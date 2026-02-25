using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkDetailsUpload;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class NetworkDetailsUploadController : Controller
    {
        private readonly ILogger<NetworkDetailsUploadController> _logger;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IS3UploadService _s3UploadService;
        private readonly ISoaService _soaProjectService;
        public NetworkDetailsUploadController(ILogger<NetworkDetailsUploadController> logger, IHeatNetworkService heatNetworkService, IUserService userService, ISessionHelper sessionHelper, IS3UploadService s3UploadService, ISoaService soaProjectService)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _s3UploadService = s3UploadService;
            _soaProjectService = soaProjectService;
        }        

        [HttpGet]
        public async Task<IActionResult> MeteringAndMonitoringStrategy()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            ViewBag.Action = "MeteringAndMonitoringStrategy";
            ViewBag.Heading = "Metering and monitoring strategy";
            ViewBag.Description1 = "Complete the metering and monitoring strategy for phase 1.";
            ViewBag.Description2 = "Upload the metering and monitoring strategy for the phase 1 of the heat network";
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            UploadedDocumentInfo? uploadedDocument = null;
            try
            {                
                if (!string.IsNullOrEmpty(hnId))
                {
                    var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                    var document = heatNetworkData?.MeteringAndMonitoringStrategy?.Documents?.FirstOrDefault();
                    if (document != null)
                    {
                        uploadedDocument = new UploadedDocumentInfo
                        {
                            FileName = document.FileName!,
                            UploadedBy = document.UploadedBy!,
                            S3Key = document.S3Key!
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve uploaded document for HN ID: {HnId}", hnId);
            }

            var model = new NetworkDetailsUploadViewModel
            {
                HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName),
                TemplateDownloadUrl = uploadedDocument?.S3Key != null
                    ? Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                        Url.Action("DownloadFile", "NetworkDetailsUpload")!,
                        "key",
                        uploadedDocument.S3Key)
                    : null,
                UploadedDocument = uploadedDocument
            };
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkDetailsUploadSessionKey, model);

            return View("UploadDocument", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MeteringAndMonitoringStrategy(IFormFile docToUpload)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            ViewBag.Action = "MeteringAndMonitoringStrategy";
            ViewBag.Heading = "Metering and monitoring strategy";
            ViewBag.Description1 = "Complete the metering and monitoring strategy for phase 1.";
            ViewBag.Description2 = "Upload the metering and monitoring strategy for the phase 1 of the heat network";

            if (docToUpload == null || docToUpload.Length == 0)
            {
                var model = _sessionHelper.GetFromSession<NetworkDetailsUploadViewModel>(HttpContext, SessionKeys.NetworkDetailsUploadSessionKey);
                ModelState.AddModelError("meteringAndMonitoringStrategy", "Please select a file to upload.");
                return View("UploadDocument", model);
            }

            var s3Key = await _s3UploadService.UploadFileAsync(docToUpload, $"NetworkDetails/{hnId}/MeteringAndMonitoringStrategy");

            // Optionally persist metadata or update project state here
            var request = new NetworkDetailsUploadDocumentRequest(hnId: hnId, uploadedBy: userId, fileName: docToUpload.FileName, s3Key: s3Key, documentType: DocumentType.MeteringAndMonitoringStrategy);
            await _heatNetworkService.UpdateDocument(request);

            _logger.LogInformation("Metering And Monitoring Strategy uploaded for HN ID: {HnId}, UploadedBy: {UserId}", hnId, userId);

            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }

        [HttpGet]
        public async Task<IActionResult> AssessmentPlan()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            ViewBag.Action = "AssessmentPlan";
            ViewBag.Heading = "Network assessment plan";
            ViewBag.Description1 = "Complete the assessment plan for phase 1.";
            ViewBag.Description2 = "Upload the network assessment plan for the phase 1 of the heat network";
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            UploadedDocumentInfo? uploadedDocument = null;
            try
            {
                if (!string.IsNullOrEmpty(hnId))
                {
                    var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                    var document = heatNetworkData?.AssessmentPlan?.Documents?.FirstOrDefault();
                    if (document != null)
                    {
                        uploadedDocument = new UploadedDocumentInfo
                        {
                            FileName = document.FileName!,
                            UploadedBy = document.UploadedBy!,
                            S3Key = document.S3Key!
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve uploaded document for HN ID: {HnId}", hnId);
            }

            var model = new NetworkDetailsUploadViewModel
            {
                HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName),
                TemplateDownloadUrl = uploadedDocument?.S3Key != null
                    ? Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                        Url.Action("DownloadFile", "NetworkDetailsUpload")!,
                        "key",
                        uploadedDocument.S3Key)
                    : null,
                UploadedDocument = uploadedDocument
            };
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkDetailsUploadSessionKey, model);

            return View("UploadDocument", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssessmentPlan(IFormFile docToUpload)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            ViewBag.Action = "AssessmentPlan";
            ViewBag.Heading = "Network assessment plan";
            ViewBag.Description1 = "Complete the assessment plan for phase 1.";
            ViewBag.Description2 = "Upload the network assessment plan for the phase 1 of the heat network";

            if (docToUpload == null || docToUpload.Length == 0)
            {
                var model = _sessionHelper.GetFromSession<NetworkDetailsUploadViewModel>(HttpContext, SessionKeys.NetworkDetailsUploadSessionKey);
                ModelState.AddModelError("assessmentPlan", "Please select a file to upload.");
                return View("UploadDocument", model);
            }

            var s3Key = await _s3UploadService.UploadFileAsync(docToUpload, $"NetworkDetails/{hnId}/AssessmentPlan");

            // Optionally persist metadata or update project state here
            var request = new NetworkDetailsUploadDocumentRequest(hnId: hnId, uploadedBy: userId, fileName: docToUpload.FileName, s3Key: s3Key, documentType: DocumentType.AssessmentPlan);
            await _heatNetworkService.UpdateDocument(request);

            _logger.LogInformation("Assessment Plan uploaded for HN ID: {HnId}, UploadedBy: {UserId}", hnId, userId);

            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }

        [HttpGet]
        public async Task<IActionResult> DesignConstructionLog()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            ViewBag.Action = "DesignConstructionLog";
            ViewBag.Heading = "Design construction log";
            ViewBag.Description1 = "Complete the design construction log for phase 1.";
            ViewBag.Description2 = "Upload the design construction log for the phase 1 of the heat network";
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            UploadedDocumentInfo? uploadedDocument = null;
            try
            {
                if (!string.IsNullOrEmpty(hnId))
                {
                    var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                    var document = heatNetworkData?.DesignConstructionLog?.Documents?.FirstOrDefault();
                    if (document != null)
                    {
                        uploadedDocument = new UploadedDocumentInfo
                        {
                            FileName = document.FileName!,
                            UploadedBy = document.UploadedBy!,
                            S3Key = document.S3Key!
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve uploaded document for HN ID: {HnId}", hnId);
            }

            var model = new NetworkDetailsUploadViewModel
            {
                HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName),
                TemplateDownloadUrl = uploadedDocument?.S3Key != null
                    ? Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                        Url.Action("DownloadFile", "NetworkDetailsUpload")!,
                        "key",
                        uploadedDocument.S3Key)
                    : null,
                UploadedDocument = uploadedDocument
            };
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkDetailsUploadSessionKey, model);

            return View("UploadDocument", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesignConstructionLog(IFormFile docToUpload)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            ViewBag.Action = "DesignConstructionLog";
            ViewBag.Heading = "Design construction log";
            ViewBag.Description1 = "Complete the design construction log for phase 1.";
            ViewBag.Description2 = "Upload the design construction log for the phase 1 of the heat network";

            if (docToUpload == null || docToUpload.Length == 0)
            {
                var model = _sessionHelper.GetFromSession<NetworkDetailsUploadViewModel>(HttpContext, SessionKeys.NetworkDetailsUploadSessionKey);
                ModelState.AddModelError("designConstructionLog", "Please select a file to upload.");
                return View("UploadDocument", model);
            }

            var s3Key = await _s3UploadService.UploadFileAsync(docToUpload, $"NetworkDetails/{hnId}/DesignConstructionLog");

            // Optionally persist metadata or update project state here
            var request = new NetworkDetailsUploadDocumentRequest(hnId: hnId, uploadedBy: userId, fileName: docToUpload.FileName, s3Key: s3Key, documentType: DocumentType.DesignConstructionLog);
            await _heatNetworkService.UpdateDocument(request);

            _logger.LogInformation("Design Construction Log uploaded for HN ID: {HnId}, UploadedBy: {UserId}", hnId, userId);

            return RedirectToAction("NetworkDetails", "HeatNetwork");
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
