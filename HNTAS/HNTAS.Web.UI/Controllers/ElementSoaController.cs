using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.ElementSoa;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetworkDetail)]
    public class ElementSoaController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly ISoaService _soaProjectService;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IUserService _userService;
        private readonly ILogger<ElementSoaController> _logger;

        public ElementSoaController(ISessionHelper sessionHelper,
            ISoaService soaProjectService,
            ILogger<ElementSoaController> logger,
            IHeatNetworkService heatNetworkService,
            IUserService userService)
        {
            _sessionHelper = sessionHelper;
            _soaProjectService = soaProjectService;
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
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
                            var elementStage = elementSoaElement.SoaStages?
                                .Find(s => s.StageId.HasValue && stageInModel.StageId.HasValue && (int)s.StageId.Value == (int)stageInModel.StageId.Value);

                            if (modelElement != null)
                            {
                                modelElement.SoaStatus = elementStage?.SoaStatus ?? "Not started";
                                modelElement.SoaStatusUpdatedAt = elementStage?.SoaStatusUpdatedAt.HasValue == true
                                    ? elementStage.SoaStatusUpdatedAt.Value.DateTime
                                    : (DateTime?)null;
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
        public async Task<IActionResult> SoaUpdateStatus([FromQuery] SoaStage stage, [FromQuery] string elementId, [FromQuery] HeatNetworkElementDisplayType elementType)
        {
            var tempDataKey = $"Soa_{elementId}_{stage}";
            string soaPhase = TempData[$"{tempDataKey}_Phase"] as string;
            string elementName = TempData[$"{tempDataKey}_Element"] as string;
            var currentStageIndex = ElementSoaHelper.GetStageIndex(stage);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey, currentStageIndex);
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            ViewBag.HnId = hnId;
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            // Set element-specific ViewBag properties
            var content = ElementSoaHelper.GetSoaElementContent(elementType);
            ViewBag.Heading = content.Heading;
            ViewBag.Description1 = content.Description1;
            ViewBag.Description2 = content.Description2;

            var selectedStatusForElement = heatNetworkData?.NetworkElements?.Elements?
                .Find(e => e.ElementId == elementId)?.SoaStages?
                .Find(s => s.StageId.HasValue && (SoaStage)s.StageId.Value == stage)?.SoaStatus;

            var model = ElementSoaHelper.GetSoaStatuses();
            model.SelectedSoaStatus = selectedStatusForElement;
            model.SoaStage = stage;
            model.ElementId = elementId;
            model.ElementName = elementName;
            model.SoaPhase = soaPhase;

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ElementSoaStatusUpdateModelSessionKey, model);

            return View("SoaUpdateStatus", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoaUpdateStatus(ElementSoaUpdateStatusViewModel model)
        {
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);
            var modelFromSession = _sessionHelper.GetFromSession<ElementSoaUpdateStatusViewModel>(HttpContext, SessionKeys.ElementSoaStatusUpdateModelSessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            if (!ModelState.IsValid)
            {
                return View(modelFromSession);
            }

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var incompleteSoa = _sessionHelper.GetFromSession<ElementSoaProgressStatusTracking>(HttpContext, SessionKeys.ElementSoaIncompleteSoaSessionKey);
            var targetStatus = NetworkDetailsStatus.InProgress;
            if (incompleteSoa != null && ((incompleteSoa.IncompleteElementId == modelFromSession?.ElementId && incompleteSoa.IncompleteSoaStageId == modelFromSession?.SoaStage) || incompleteSoa.AllElementsCompleted))
            {
                targetStatus = NetworkDetailsStatus.Complete;
            }
            var request = new ElementSoaStatusUpdateRequest(hnId: hnId!, stage: modelFromSession?.SoaStage, elementId: model?.ElementId, soaStatus: model?.SelectedSoaStatus, soaStatusUpdatedBy: userId, elementSoaStatus: targetStatus, soaPhase: modelFromSession?.SoaPhase, elementDisplayName: modelFromSession?.ElementName);
            await _soaProjectService.UpdateElementSoaStatus(request);
            ClearSoaSpecificSession();
            return RedirectToAction("SoaStages", "ElementSoa", targetFragment);
        }

        [HttpGet]
        public IActionResult AssessorOnboarding()
        {
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> SearchAssessor(string q)
        {
            try
            {
                if (string.IsNullOrEmpty(q) || q.Length < 2)
                {
                    return Json(new List<string>());
                }

                var results = await _userService.GetActiveAssessors(q);
                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assessor suggestions");
                return Json(new { error = "Internal server error" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectedAssessorOnboarding(string firstName, string lastName, string emailId)
        {
            if (string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(emailId))
            {
                TempData["ErrorMessage"] = "Please select an assessor before continuing.";
                return RedirectToAction("AssessorOnboarding");
            }

            var model = new AssessorDetails
            {
                FirstName = firstName,
                LastName = lastName,
                Email = emailId
            };

            // Store in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorDetailsSessionKey, model);
            return RedirectToAction("AssessorSelectElements");
        }
        
        [HttpGet]
        public async Task<IActionResult> AssessorSelectElementsAsync()
        {            
            this.ShowBackButton("SearchAssessor", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;

            var model = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectElementsViewModelSessionKey);

            if (model != null)
            {
                return View(model);
            }

            model = new AssessorSelectElementsViewModel();
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            var selectedNetworkElements = heatNetworkData?.NetworkElements;

            if (selectedNetworkElements != null)
            {
                foreach (var item in selectedNetworkElements.Elements!)
                {
                    model.ElementOptions?.Add(new AssessorSelectElementsOption { Label = item.NetworkElementInstanceName!, ElementId = item.ElementId!});
                }                
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorSelectElementsViewModelSessionKey, model);

            return View("AssessorSelectElements", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssessorSelectElements(AssessorSelectElementsViewModel model)
        {
            this.ShowBackButton("SearchAssessor", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var modelFromSession = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectElementsViewModelSessionKey);
            if (!ModelState.IsValid)
            {
                
                return View("AssessorSelectElements", modelFromSession);
            }

            foreach (var item in model.SelectedElementIds!)
            {
                foreach(var option in modelFromSession?.ElementOptions!)
                {                     
                    if (option.ElementId == item)
                    {
                        model.SelectedElementLabel?.Add(option.Label);
                    }
                }
            }
            // Store selected element IDs in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorSelectedElementSessionKey, model);
            return RedirectToAction("AssessmentSelection", "ElementSoa");
        }

        [HttpGet]
        public IActionResult AssessmentSelection()
        {
            this.ShowBackButton("AssessorSelectElements", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;

            var model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionViewModelSessionKey) ?? new AssessorAssessmentSelectionViewModel();
            model.AssessmentOptions = ElementSoaHelper.GetAssessmentOptions();
            
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionViewModelSessionKey, model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssessmentSelection(AssessorAssessmentSelectionViewModel model)
        {
            this.ShowBackButton("AssessorSelectElements", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            if (!ModelState.IsValid)
            {
                model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionViewModelSessionKey);
                return View("AssessmentSelection", model);
            }
            // Store selected assessment type in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionViewModelSessionKey, model);
            return RedirectToAction("AssessorElementSelectionOverview", "ElementSoa");
        }

        [HttpGet]
        public async Task<IActionResult> AssessorElementSelectionOverviewAsync()
        {
            this.ShowBackButton("AssessmentSelection", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var phase = heatNetworkData?.Phase;
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var assessmentSelectionModel = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionViewModelSessionKey);
            var model = new AssessorElementSelectionOverviewModel
            {
                AssessorAssessment = assessmentSelectionModel!,
                AssessorSelectedElements = selectedElements!,
                AssessorDetails = assessorDetails!,
                HeatNetworkPhase = phase ?? string.Empty,
                HeatNetworkStage = ElementSoaHelper.GetStageFromPhase(phase!)
            };
            return View(model);
        }

        private void ClearSoaSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ElementSoaStatusUpdateModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ElementSoaIncompleteSoaSessionKey);
        }
    }
}
