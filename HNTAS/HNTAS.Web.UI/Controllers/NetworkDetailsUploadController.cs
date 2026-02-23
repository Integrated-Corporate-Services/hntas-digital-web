using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.Soa;
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
        public NetworkDetailsUploadController(ILogger<NetworkDetailsUploadController> logger, IHeatNetworkService heatNetworkService, IUserService userService, ISessionHelper sessionHelper, IS3UploadService s3UploadService)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _s3UploadService = s3UploadService;
        }

        [HttpGet]
        public IActionResult UploadDocument([FromQuery] string hnId)
        {

            this.ShowBackButton("AddAssessmentPlanSoa", "SOA");

            var phaseNumber = 1;
            var model = new UploadAssessmentPlanViewModel
            {
                HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName),
                PhaseNumber = phaseNumber,
                TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { phaseNumber })
            };

            return View("UploadDocument", model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAssessmentPlan(int phase, IFormFile assessmentPlan)
        {
            phase = phase + 1;
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            if (assessmentPlan == null || assessmentPlan.Length == 0)
            {
                ModelState.AddModelError("assessmentPlan", "Please select a file to upload.");
                return View(new UploadAssessmentPlanViewModel
                {
                    PhaseNumber = phase,
                    TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { phase })
                });
            }

            var s3Key = await _s3UploadService.UploadFileAsync(assessmentPlan, $"soa/{hnId}/{phase}/AssessmentPlan");

            // Optionally persist metadata or update project state here
            var request = new UpdateDocumentRequest(hnId: hnId, phase: (SoaPhase)phase, uploadedBy: userId, fileName: assessmentPlan.FileName, s3Key: s3Key, documentType: DocumentType.Assessment);
            //await _soaProjectService.UpdateDocument(request);

            _logger.LogInformation("Assessment plan uploaded for HN ID: {HnId}, Phase: {Phase}, UploadedBy: {UserId}", hnId, phase, userId);

            return RedirectToAction("SubmitAssessmentPlan");
        }


        public async Task<IActionResult> SubmitAssessmentPlan(int phaseIndex = 0)
        {
            this.ShowBackButton("UploadAssessmentPlan", "SOA", new { phase = phaseIndex });

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var heatNetworkResponse = await _heatNetworkService.GetAsync(hnId);

            var model = new SubmitAssessmentPlanViewModel // Updated reference
            {
                DocumentName = heatNetworkResponse.Soa.JourneyData.AssessmentDocs.FirstOrDefault()?.FileName, //"MyAssessmentPlan.docx",
                PhaseNumber = phaseIndex + 1,
                Steps = StaticSoaSteps.GetSteps(SoaSteps.SubmitSoa, Url)
            };
            return View(model);
        }
    }
}
