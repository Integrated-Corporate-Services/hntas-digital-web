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
        [ValidateAntiForgeryToken]
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
            var eligibleIndex = phase == "Design" ? 1 : phase == "Construction" ? 2 : 0;
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var model = ElementSoaHelper.GetElementSoaViewModel(eligibleIndex, currentStageIndex, networkElements);

            foreach (var stageInModel in model.Stages)
            {                
                if (networkElements != null)
                {
                    foreach (var elementSoaElement in networkElements)
                    {
                        if (elementSoaElement.ElementId == stageInModel.Elements?.FirstOrDefault(e => e.ElementId == elementSoaElement.ElementId)?.ElementId)
                        {
                            var modelElement = stageInModel.Elements?.Find(e => e.ElementId == elementSoaElement.ElementId);
                            var doc = elementSoaElement.SoaStages?
                                .Find(s => s.StageId.HasValue && stageInModel.StageId.HasValue && (int)s.StageId.Value == (int)stageInModel.StageId.Value)
                                ?.Document;
                            if (modelElement != null)
                            {
                                modelElement.Document = doc;
                            }
                        }
                    }
                }
            }
            var incompleteSoa = ElementSoaHelper.GetElementSoaProgressStatusTracking(model);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ElementSoaIncompleteSoaSessionKey, incompleteSoa);
            return View("SoaStages", model);
        }

        [HttpGet]
        public async Task<IActionResult> SoaStagesToUpload([FromQuery] SoaStage stage, [FromQuery] string elementId, [FromQuery] HeatNetworkElementDisplayType elementType)
        {
            var currentStageIndex = ElementSoaHelper.GetStageIndex(stage);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey, currentStageIndex);
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);
            @ViewBag.Action = "SoaStagesToUpload";

            

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            // Set element-specific ViewBag properties
            var content = ElementSoaHelper.GetSoaElementContent(elementType);
            ViewBag.Heading = content.Heading;
            ViewBag.Description1 = content.Description1;
            ViewBag.Description2 = content.Description2;

            var DocumentForElement = heatNetworkData?.NetworkElements?.Elements?
                .Find(e => e.ElementId == elementId)?.SoaStages?
                .Find(s => s.StageId.HasValue && (SoaStage)s.StageId.Value == stage)?.Document;

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
                                    Url.Action("DownloadFile", "ElementSoa")!,
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoaStagesToUpload(IFormFile docToUpload)
        {
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);
            var model = _sessionHelper.GetFromSession<ElementSoaUploadViewModel>(HttpContext, SessionKeys.ElementSoaUploadViewModelSessionKey);
            var elementType = model?.Type;
            // Set element-specific ViewBag properties
            var content = ElementSoaHelper.GetSoaElementContent(elementType);
            ViewBag.Heading = content.Heading;
            ViewBag.Description1 = content.Description1;
            ViewBag.Description2 = content.Description2;
            if (docToUpload == null || docToUpload.Length == 0)
            {

                ModelState.AddModelError("assessmentPlan", "Please select a file to upload.");
                return View("SoaUpload", model);
            }
            var fileExtension = Path.GetExtension(docToUpload.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".pdf")
            {
                ModelState.AddModelError("excelFile", "The selected file must be an XLSX or PDF");
                return View("SoaUpload", model);
            }

            var incompleteSoa = _sessionHelper.GetFromSession<ElementSoaProgressStatusTracking>(HttpContext, SessionKeys.ElementSoaIncompleteSoaSessionKey);

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            var s3Key = await _s3UploadService.UploadFileAsync(docToUpload, $"NetworkDetails/{hnId}/Soa/{model?.SoaStage}/{model?.ElementId}");

            var targetStatus = NetworkDetailsStatus.InProgress;
            if (incompleteSoa != null && ((incompleteSoa.IncompleteElementId == model?.ElementId && incompleteSoa.IncompleteSoaStageId == model?.SoaStage) || incompleteSoa.AllElementsCompleted))
            {                
                targetStatus = NetworkDetailsStatus.Complete;
            }
            var request = new ElementSoaUploadDocumentRequest(hnId: hnId, uploadedBy: userId, fileName: docToUpload.FileName, s3Key: s3Key, documentType: DocumentType.Soa, stage: model?.SoaStage, elementId: model?.ElementId, elementSoaStatus: targetStatus);
            await _soaProjectService.UpdateDocumentSoa(request);

            _logger.LogInformation("Assessment Plan uploaded for HN ID: {HnId}, UploadedBy: {UserId}", hnId, userId);
            
            return RedirectToAction("SoaStages", "ElementSoa", targetFragment);
        }

        public async Task<IActionResult> DownloadFile([FromQuery] string key)
        {
            var stream = await _s3UploadService.GetFileAsync(key);
            if (stream == null)
                return NotFound();

            return File(stream, "application/octet-stream", Path.GetFileName(key));
        }        
    }
}
