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
            var phase = heatNetworkData?.Phase;

            var networkElements = heatNetworkData?.NetworkElements?.Elements;
            var elementSoa = heatNetworkData?.ElementSoa;
            var elementSoaElements = elementSoa?.Elements;
            var eligibleIndex = phase == "Design" ? 1 : phase == "Construction" ? 2 : 0;

            var model = new ElementSoaViewModel
            {
                EligibleStageIndex = eligibleIndex,
                Status = NetworkDetailsStatus.Incomplete,
                Stages = new List<SoaStagesView>
                {
                    new SoaStagesView
                    {
                        Name = "Stage 1",
                        StageId = SoaStage.Stage1,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = eligibleIndex == 0,
                        Title = "Feasibility (Concept Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 2",
                        StageId = SoaStage.Stage2,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = eligibleIndex == 1,
                        Title = "Design (Developed Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 3",
                        StageId = SoaStage.Stage3,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = false,
                        Title = "Design (Technical Design)"
                    },                    
                    new SoaStagesView
                    {
                        Name = "Stage 4",
                        StageId = SoaStage.Stage4,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = false,
                        Title = "Construction (Construction Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 5",
                        StageId = SoaStage.Stage5,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = false,
                        Title = "Construction (Installation)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 6",
                        StageId = SoaStage.Stage6,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = false,
                        Title = "Construction (Commissioning)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 7",
                        StageId = SoaStage.Stage7,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = false,
                        Title = "Operation (Operation and Maintenance)"
                    },

                }
            };

            foreach (var stageInModel in model.Stages)
            {
                //if (elementSoaStages != null)
                //{
                //    foreach (var modelElement in stageInModel.Elements)
                //    {
                //        foreach (var elementSoaStage in elementSoaStages)
                //        {
                //            if (elementSoaStage.Stage == stageInModel.Stage)
                //            {
                //                var elementInStage = elementSoaStage.Elements?.Find(e => e.ElementId == modelElement.ElementId);
                //                if (elementInStage != null)
                //                {
                //                    modelElement.Documents = elementInStage.Documents ?? [];
                //                }
                //            }
                //        }
                //    }
                //}
                
                if (elementSoaElements != null)
                {
                    foreach (var elementSoaElement in elementSoaElements)
                    {
                        if (elementSoaElement.ElementId == stageInModel.Elements?.FirstOrDefault(e => e.ElementId == elementSoaElement.ElementId)?.ElementId)
                        {
                            var modelElement = stageInModel.Elements?.Find(e => e.ElementId == elementSoaElement.ElementId);
                            var doc = elementSoaElement.Stages?.Find(s => s.StageId == stageInModel.StageId)?.Document;
                            if (modelElement != null)
                            {
                                modelElement.Document = doc;
                            }
                        }
                    }
                }
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ElementSoaViewModelSessionKey, model);
            return View("SoaStages", model);
        }

        [HttpGet]
        public IActionResult SoaStagesToUpload([FromQuery] SoaStage stage, [FromQuery] string elementId, [FromQuery] HeatNetworkElementDisplayType elementType)
        {
            this.ShowBackButton("SoaStages", "ElementSoa");
            var soaStagesModel = _sessionHelper.GetFromSession<ElementSoaViewModel>(HttpContext, SessionKeys.ElementSoaViewModelSessionKey);
            @ViewBag.Action = "SoaStagesToUpload";

            // Set element-specific ViewBag properties
            SetSoaElementsViewBag(elementType);            
            var DocumentForElement = soaStagesModel?.Stages?
                .Find(s => s.StageId == stage)?.Elements?
                .Find(e => e.ElementId == elementId)?.Document;

            var document = DocumentForElement;
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
                UploadedDocument = uploadedDocument,
                Type = elementType
            };
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ElementSoaUploadViewModelSessionKey, model);
            
            return View("SoaUpload", model);
        }

        [HttpPost]
        public async Task<IActionResult> SoaStagesToUpload(IFormFile docToUpload)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<ElementSoaUploadViewModel>(HttpContext, SessionKeys.ElementSoaUploadViewModelSessionKey);
            var elementType = model?.Type;
            // Set element-specific ViewBag properties
            SetSoaElementsViewBag(elementType);
            if (docToUpload == null || docToUpload.Length == 0)
            {

                ModelState.AddModelError("assessmentPlan", "Please select a file to upload.");
                return View("SoaUpload", model);
            }
            //var fileExtension = Path.GetExtension(docToUpload.FileName).ToLower();
            //if (fileExtension != ".xlsx" && fileExtension != ".pdf")
            //{
            //    ModelState.AddModelError("excelFile", "The selected file must be an XLSX or PDF");
            //    return View("SoaUpload", model);
            //}
            
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            

            var s3Key = await _s3UploadService.UploadFileAsync(docToUpload, $"NetworkDetails/{hnId}/Soa/{model?.SoaStage}/{model?.ElementId}");

            // Optionally persist metadata or update project state here
            var request = new ElementSoaUploadDocumentRequest(hnId: hnId, uploadedBy: userId, fileName: docToUpload.FileName, s3Key: s3Key, documentType: DocumentType.Soa, stage: model?.SoaStage, elementId: model?.ElementId);
            await _soaProjectService.UpdateDocumentSoa(request);

            _logger.LogInformation("Assessment Plan uploaded for HN ID: {HnId}, UploadedBy: {UserId}", hnId, userId);

            return RedirectToAction("SoaStages", "ElementSoa");
        }

        private void SetSoaElementsViewBag(HeatNetworkElementDisplayType? elementType)
        {
            (ViewBag.Heading, ViewBag.Description1, ViewBag.Description2) = elementType switch
            {
                HeatNetworkElementDisplayType.EnergyCentre => (
                    "Upload the statement of applicability (SOA) for the energy centre",
                    "Intro to Energy centre...",
                    "Upload the statement of applicability (SOA) for the energy centre"
                ),
                HeatNetworkElementDisplayType.DistributionNetwork => (
                    "Upload the statement of applicability (SOA) for the distribution network",
                    "Intro to Distribution network...",
                    "Upload the statement of applicability (SOA) for the distribution network"
                ),
                HeatNetworkElementDisplayType.ThermalSubStation => (
                    "Upload the statement of applicability (SOA) for the thermal substation",
                    "Intro to Thermal substation...",
                    "Upload the statement of applicability (SOA) for the thermal substation"
                ),
                HeatNetworkElementDisplayType.ConsumerConnections => (
                    "Upload the statement of applicability (SOA) for the consumer connections",
                    "Intro to Consumer connections...",
                    "Upload the statement of applicability (SOA) for the consumer connections"
                ),
                HeatNetworkElementDisplayType.CommunalDistributionNetwork => (
                    "Upload the statement of applicability (SOA) for the communal distribution network",
                    "Intro to Communal distribution network...",
                    "Upload the statement of applicability (SOA) for the communal distribution network"
                ),
                HeatNetworkElementDisplayType.ConsumerHeatSystems => (
                    "Upload the statement of applicability (SOA) for the consumer heat systems",
                    "Intro to Consumer heat systems...",
                    "Upload the statement of applicability (SOA) for the consumer heat systems"
                )
            };
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
                    Type = element.Type,
                    Name = Utility.GetDefaultNetworkElementOptions().Find(a => a.Id.ToString() == element.Type.ToString()).Label
                });
            }
            return soaElements;
        }
    }
}
