using DocumentFormat.OpenXml.Wordprocessing;
using HNTAS.Api.Client.Api;
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
        private readonly IAssessorApi _assessorApi;
        private readonly ILogger<ElementSoaController> _logger;

        public ElementSoaController(ISessionHelper sessionHelper,
            ISoaService soaProjectService,
            ILogger<ElementSoaController> logger,
            IHeatNetworkService heatNetworkService,
            IUserService userService,
            IAssessorApi assessorApi)
        {
            _sessionHelper = sessionHelper;
            _soaProjectService = soaProjectService;
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
            _assessorApi = assessorApi;
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
            ClearSoaAssessorSpecificSession();

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
                                modelElement.AssessorEmailId = elementStage?.Assessor?.Email;
                                modelElement.AssessorFirstName = elementStage?.Assessor?.FirstName;
                                modelElement.AssessorLastName = elementStage?.Assessor?.LastName;
                                modelElement.Assessment = elementStage?.Assessor?.Assessment;
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

            var request = new ElementSoaStatusUpdateRequest(hnId: hnId!, stage: modelFromSession?.SoaStage, elementId: model?.ElementId, soaStatus: model?.SelectedSoaStatus, soaStatusUpdatedBy: userId, elementSoaStatus: targetStatus, soaPhase: modelFromSession?.SoaPhase, elementDisplayName: modelFromSession?.ElementName);
            await _soaProjectService.UpdateElementSoaStatus(request);
            ClearSoaStatusUpdateSpecificSession();
            return RedirectToAction("SoaStages", "ElementSoa", targetFragment);
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult PrepareAssessorOnboarding([FromQuery] SoaStage stage, [FromQuery] string selectedAssessor, [FromQuery] string selectedAssessorFirstName, [FromQuery] string selectedAssessorLastName, [FromQuery] string selectedAssessorEmail)
        {
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding, stage);

            var selectedAssessorDetails = new AssessorDetails
            {
                FirstName = selectedAssessorFirstName,
                LastName = selectedAssessorLastName,
                Email = selectedAssessorEmail,
                FullNameWithEmail = selectedAssessor
            };
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.DefaultSelectedAssessor, selectedAssessorDetails);
            return RedirectToAction("AssessorOnboarding", "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessorOnboarding()
        {
            var stage = _sessionHelper.GetFromSession<SoaStage>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            var selectedAssessor = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.DefaultSelectedAssessor);
            int currentStageIndex;
            if (stage == 0)
            {
                currentStageIndex = _sessionHelper.GetFromSession<int>(HttpContext, SessionKeys.CurrentStageIndexSessionKey);
            }
            else
            {
                currentStageIndex = ElementSoaHelper.GetStageIndex(stage);
            }

            var selectedAssessorFullNameWithEmail = selectedAssessor?.FullNameWithEmail;
            if (string.IsNullOrEmpty(selectedAssessorFullNameWithEmail))
            {
                var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
                if (assessorDetails != null)
                {
                    selectedAssessorFullNameWithEmail = $"{assessorDetails.FirstName} {assessorDetails.LastName} ({assessorDetails.Email})";
                }
            }
                
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey, currentStageIndex);
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            ViewBag.SelectedAssessor = selectedAssessorFullNameWithEmail;            
            return View();
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public async Task<JsonResult> SearchAssessor(string q)
        {
            try
            {
                if (string.IsNullOrEmpty(q) || q.Length < 2)
                {
                    return Json(new List<string>());
                }

                var results = await _assessorApi.ApiAssessorSearchGetAsync(q);
                var assessors = results.Ok();
                _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorSearchResultsSessionKey);
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorSearchResultsSessionKey, assessors);
                return Json(assessors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assessor suggestions");
                return Json(new { error = "Internal server error" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectedAssessorOnboarding(string firstName, string lastName, string emailId, string fullNameFromInput)
        {
            
            var selectedAssessor = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.DefaultSelectedAssessor);
            
            if (fullNameFromInput == null)
            {
                var stage = _sessionHelper.GetFromSession<SoaStage?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
                TempData["ErrorMessage"] = "Please select an assessor before continuing.";
                return RedirectToAction("AssessorOnboarding", "ElementSoa");
            }
            var assessorSearchResults = _sessionHelper.GetFromSession<List<AssessorSearchResult>>(HttpContext, SessionKeys.AssessorSearchResultsSessionKey);
            if (selectedAssessor?.FullNameWithEmail?.ToLower() != fullNameFromInput?.ToLower())
            {
                
                var isCorrectAssessor = assessorSearchResults?.Exists(a => a.FullNameWithEmail?.ToLower() == fullNameFromInput?.ToLower());

                if (!isCorrectAssessor ?? false)
                {
                    var stage = _sessionHelper.GetFromSession<SoaStage?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
                    TempData["ErrorMessage"] = "Please select an assessor before continuing.";
                    return RedirectToAction("AssessorOnboarding", "ElementSoa");
                }
            }

            var assessorFromList = assessorSearchResults?.FirstOrDefault(a => a.FullNameWithEmail?.ToLower() == fullNameFromInput?.ToLower());

            var model = new AssessorDetails
            {
                FirstName = assessorFromList?.FirstName! ?? selectedAssessor?.FirstName!,
                LastName = assessorFromList?.LastName! ?? selectedAssessor?.LastName!,
                Email = assessorFromList?.Email! ?? selectedAssessor?.Email!
            };            

            // Store in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorDetailsSessionKey, model);
            return RedirectToAction("AssessorSelectElements");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public async Task<IActionResult> AssessorSelectElementsAsync()
        {
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            var stage = _sessionHelper.GetFromSession<SoaStage?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            var selectedAssessor = $"{assessorDetails?.FirstName} {assessorDetails?.LastName} ({assessorDetails?.Email})";
            //this.ShowBackButton("AssessorOnboarding", "ElementSoa", new {stage, selectedAssessor});
            this.ShowBackButton("AssessorOnboarding", "ElementSoa");
            ViewBag.SelectedAssessor = selectedAssessor;
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;

            var model = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
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
                    var assignedAssessor = item.SoaStages?
                        .Select(s => s.Assessor)
                        .FirstOrDefault(a => a != null);
                    
                    model.ElementOptions?.Add(new AssessorSelectElementsOption 
                    { 
                        Label = item.NetworkElementInstanceName!, 
                        ElementId = item.ElementId!,
                        AssignedAssessorName = assignedAssessor != null ? $"(Assessor Assigned: {assignedAssessor.FirstName} {assignedAssessor.LastName})" : ""
                    });
                }                
            }            
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorSelectedElementSessionKey, model);
            return View("AssessorSelectElements", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssessorSelectElements(AssessorSelectElementsViewModel model)
        {
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            var stage = _sessionHelper.GetFromSession<SoaStage?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            var selectedAssessor = $"{assessorDetails?.FirstName} {assessorDetails?.LastName} ({assessorDetails?.Email})";
            //this.ShowBackButton("AssessorOnboarding", "ElementSoa", new { stage, selectedAssessor });
            this.ShowBackButton("AssessorOnboarding", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;            
            var modelFromSession = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
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
            model.ElementOptions = modelFromSession?.ElementOptions;
            // Store selected element IDs in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorSelectedElementSessionKey, model);
            return RedirectToAction("AssessmentSelection", "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
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

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
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
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorElementSelectionOverviewModelSessionKey, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssessorElementSelectionOverviewConfirm(AssessorElementSelectionOverviewModel model)
        {
            this.ShowBackButton("AssessmentSelection", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var assessmentSelectionModel = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionViewModelSessionKey);
            
            var requestModel = new ElementSoaAssignAssessorRequest
            {
                HnId = hnId!,
                AssessorEmail = assessorDetails?.Email,
                AssessorFirstName = assessorDetails?.FirstName,
                AssessorLastName = assessorDetails?.LastName,
                ElementIds = selectedElements?.SelectedElementIds,
                Assessment = assessmentSelectionModel?.SelectedAssessmentOption,
                UpdatedBy = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey)

            };
            await _soaProjectService.AssignAssessor(requestModel);            
            return RedirectToAction("AssessorAssignedConfirmation", "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessorAssignedConfirmation()
        {
            var model = _sessionHelper.GetFromSession<AssessorElementSelectionOverviewModel>(HttpContext, SessionKeys.AssessorElementSelectionOverviewModelSessionKey);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssessorAssignedConfirmationOk()
        {
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var targetFragment = string.Concat("stage-", currentStageIndex);
            ClearSoaAssessorSpecificSession();
            return RedirectToAction("SoaStages", "ElementSoa", targetFragment);
        }

        private void ClearSoaStatusUpdateSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ElementSoaStatusUpdateModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ElementSoaIncompleteSoaSessionKey);
        }

        private void ClearSoaAssessorSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorAssessmentSelectionViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorElementSelectionOverviewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.DefaultSelectedAssessor);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorSearchResultsSessionKey);
        }
    }
}
