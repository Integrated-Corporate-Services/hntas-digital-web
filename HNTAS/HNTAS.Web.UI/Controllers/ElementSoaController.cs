using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.ElementSoa;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.NetworkDetailsUpload;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class ElementSoaController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly ISoaService _soaProjectService;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly ILogger<ElementSoaController> _logger;
        private readonly IS3UploadService _s3UploadService;

        public ElementSoaController(ISessionHelper sessionHelper,
            ISoaService soaProjectService,
            ILogger<ElementSoaController> logger,
            IS3UploadService s3UploadService,
            IHeatNetworkService heatNetworkService)
        {
            _sessionHelper = sessionHelper;
            _soaProjectService = soaProjectService;
            _logger = logger;
            _s3UploadService = s3UploadService;
            _heatNetworkService = heatNetworkService;
        }

        [HttpGet]
        public IActionResult UnderstandingSoa()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            return View();
        }

        [HttpPost]
        public IActionResult SubmitUnderstandingSoa()
        {
            return RedirectToAction("SoaStages");
        }
        [HttpGet]
        public async Task<IActionResult> SoaStages()
        {
            this.ShowBackButton("UnderstandingSoa", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            ViewBag.HnId = hnId;

            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            var networkElements = heatNetworkData?.NetworkElements?.Elements;
            var elementSoa = heatNetworkData?.ElementSoa;
            var elementSoaStages = elementSoa?.Stages;

            //var soaElements = GetElementsForStage(networkElements);

            var model = new ElementSoaViewModel
            {
                Status = NetworkDetailsStatus.Incomplete,
                Stages = new List<SoaStagesView>
                {
                    new SoaStagesView
                    {
                        Name = "Stage 1",
                        Stage = SoaStage.Stage1,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = true,
                        Title = "Feasibility (Concept Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 2",
                        Stage = SoaStage.Stage2,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = false,
                        Title = "Design (Developed Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 3",
                        Stage = SoaStage.Stage3,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = false,
                        Title = "Design (Technical Design)"
                    }
                }
            };

            foreach (var stageInModel in model.Stages)
            {
                if (elementSoaStages != null)
                {
                    foreach (var modelElement in stageInModel.Elements)
                    {
                        foreach (var elementSoaStage in elementSoaStages)
                        {
                            if (elementSoaStage.Stage == stageInModel.Stage)
                            {
                                var elementInStage = elementSoaStage.Elements?.Find(e => e.ElementId == modelElement.ElementId);
                                if (elementInStage != null)
                                {
                                    modelElement.Documents = elementInStage.Documents ?? [];
                                }
                            }
                        }
                    }
                }
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ElementSoaViewModelSessionKey, model);
            return View("SoaStages", model);
        }

        [HttpGet]
        public IActionResult SoaStagesToUpload([FromQuery] SoaStage stage, [FromQuery] string elementId)
        {
            var soaStagesModel = _sessionHelper.GetFromSession<ElementSoaViewModel>(HttpContext, SessionKeys.ElementSoaViewModelSessionKey);
            @ViewBag.Action = "SoaStagesToUpload";
            var DocumentForElement = soaStagesModel?.Stages?
                .Find(s => s.Stage == stage)?.Elements?
                .Find(e => e.ElementId == elementId)?.Documents;

            var document = DocumentForElement?.FirstOrDefault();
            UploadedDocumentInfo? uploadedDocument = null;

            if (document != null)
            {
                uploadedDocument = new UploadedDocumentInfo
                {
                    FileName = document.FileName,
                    UploadedBy = document.UploadedBy!,
                    S3Key = document.S3Key,
                    DocumentUrl = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                                    Url.Action("DownloadFile", "NetworkDetailsUpload")!,
                                    "key",
                                    document.S3Key!)

                };
            }
            var model = new ElementSoaUploadViewModel
            {
                ElementId = elementId,
                SoaStage = stage,
                HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName),                
                UploadedDocument = uploadedDocument
            };
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ElementSoaUploadViewModelSessionKey, model);
            
            return View("SoaUpload", model);
        }

        [HttpPost]
        public async Task<IActionResult> SoaStagesToUpload(IFormFile docToUpload)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            ViewBag.Action = "AssessmentPlan";
            ViewBag.Heading = "Network assessment plan";
            ViewBag.Description1 = "Complete the assessment plan for phase 1.";
            ViewBag.Description2 = "Upload the network assessment plan for the phase 1 of the heat network";

            var model = _sessionHelper.GetFromSession<ElementSoaUploadViewModel>(HttpContext, SessionKeys.ElementSoaUploadViewModelSessionKey);
            if (docToUpload == null || docToUpload.Length == 0)
            {
                
                ModelState.AddModelError("assessmentPlan", "Please select a file to upload.");
                return View("UploadDocument", model);
            }

            var s3Key = await _s3UploadService.UploadFileAsync(docToUpload, $"NetworkDetails/{hnId}/Soa/{model?.SoaStage}/{model?.ElementId}");

            // Optionally persist metadata or update project state here
            var request = new ElementSoaUploadDocumentRequest(hnId: hnId, uploadedBy: userId, fileName: docToUpload.FileName, s3Key: s3Key, documentType: DocumentType.AssessmentPlan, stage: model?.SoaStage, elementId: model?.ElementId);
            await _soaProjectService.UpdateDocumentSoa(request);

            _logger.LogInformation("Assessment Plan uploaded for HN ID: {HnId}, UploadedBy: {UserId}", hnId, userId);

            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }

        private List<SoaElementsView> GetElementsForStage(List<Element>? elements)
        {
            var soaElements = new List<SoaElementsView>();
            foreach (var element in elements ?? [])
            {
                soaElements.Add(new SoaElementsView
                {
                    ElementId = element.ElementId,
                    //ElementType = element.ElementType,
                    //Type = element.Type,
                    Name = Utility.GetDefaultNetworkElementOptions().Find(a => a.Id.ToString() == element.Type.ToString()).Label
                });
            }
            return soaElements;
        }
    }
}
