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
        private readonly IAssessorApi _assessorApi;
        private readonly ILogger<ElementSoaController> _logger;

        public ElementSoaController(ISessionHelper sessionHelper,
            ISoaService soaProjectService,
            ILogger<ElementSoaController> logger,
            IHeatNetworkService heatNetworkService,
            IAssessorApi assessorApi)
        {
            _sessionHelper = sessionHelper;
            _soaProjectService = soaProjectService;
            _logger = logger;
            _heatNetworkService = heatNetworkService;
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
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            ClearSoaAssessorSpecificSession();

            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var phase = heatNetworkData?.Phase;

            var networkElements = heatNetworkData?.NetworkElements?.ElementsGroup;
            var eligibleIndex = phase == "Design" ? 1 : phase == "Construction" ? 2 : 0;
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var model = ElementSoaHelper.GetElementSoaViewModel(eligibleIndex, currentStageIndex, networkElements);

            foreach (var stageInModel in model.Stages)
            {
                if (networkElements != null)
                {
                    foreach (var elementSoaElement in networkElements)
                    {
                        if (elementSoaElement.ElementType == stageInModel.Elements?.FirstOrDefault(e => e.ElementType == elementSoaElement.ElementType)?.ElementType)
                        {
                            var modelElement = stageInModel.Elements?.Find(e => e.ElementType == elementSoaElement.ElementType);
                            var elementStage = elementSoaElement.SoaStages?
                                .Find(s => s.StageId.HasValue && stageInModel.StageId.HasValue && (int)s.StageId.Value == (int)stageInModel.StageId.Value);

                            if (modelElement != null)
                            {
                                if (elementStage?.SoaStatuses == null)
                                {
                                    modelElement.SoaStatuses = new List<SoaStatusWithCount>() { 
                                        new SoaStatusWithCount { SoaStatus = SoaStatus.NotStarted, Count = elementSoaElement.Count },
                                    };
                                }
                                else
                                {
                                    modelElement.SoaStatuses = elementStage?.SoaStatuses;
                                }                                    
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
            return View("SoaStages", model);
        }

        [HttpGet]
        public async Task<IActionResult> SoaUpdateStatus([FromQuery] SoaStage stage, [FromQuery] string elementType, [FromQuery] HeatNetworkElementType elementDisplayType)
        {
            var tempDataKey = $"Soa_{elementType}_{stage}";
            string soaPhase = TempData[$"{tempDataKey}_Phase"] as string;
            string elementName = TempData[$"{tempDataKey}_Element"] as string;
            var currentStageIndex = ElementSoaHelper.GetStageIndex(stage);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey, currentStageIndex);
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            // Set element-specific ViewBag properties
            var content = ElementSoaHelper.GetSoaElementContent(elementDisplayType);
            ViewBag.Heading = content;            
            var selectedNetworkElement = heatNetworkData?.NetworkElements?.ElementsGroup?.Where(e => e.ElementType == elementType).FirstOrDefault();
            
            var model = new ElementSoaUpdateStatusViewModel();            
            model.SoaStage = stage;
            model.ElementType = elementType;
            model.ElementName = elementName;
            model.SoaPhase = soaPhase;
            model.ElementDisplayType = elementDisplayType;
            model.ElementCount = selectedNetworkElement?.Count;
            model.SoaStatusOptions = ElementSoaHelper.GetSoaStatuses();            

            var soaStatusesForStage = selectedNetworkElement?.SoaStages?
                .Where(s => s.StageId.HasValue && (SoaStage)s.StageId.Value == stage)
                .SelectMany(s => s.SoaStatuses!)
                .ToList();

            if (soaStatusesForStage == null || soaStatusesForStage.Count == 0)
            {
                // If there are no statuses for the element at the current stage, initialize with NotStarted
                soaStatusesForStage = new List<SoaStatusWithCount>
                {
                    new SoaStatusWithCount { SoaStatus = SoaStatus.NotStarted, Count = selectedNetworkElement?.Count }
                };
            }

            model.SelectedSoaStatusOptions = soaStatusesForStage != null
                ? soaStatusesForStage.Select(s => (SoaStatus)s.SoaStatus!).ToList()
                : new List<SoaStatus>();

            model.SoaStatusOptions.ForEach(option =>
            {
                var count = soaStatusesForStage
                    .Where(s => s.SoaStatus == option.Id)
                    .Sum(s => s.Count);
                model.SoaStatusCounts[option.Id] = count == 0 ? null : count;
            });            

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ElementSoaStatusUpdateModelSessionKey, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoaUpdateStatus(ElementSoaUpdateStatusViewModel model)
        {
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaStages", "ElementSoa", targetFragment);            
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            // Set element-specific ViewBag properties
            var content = ElementSoaHelper.GetSoaElementContent(model.ElementDisplayType);
            ViewBag.Heading = content;

            var totalCountFromElement = model.ElementCount ?? 0;

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var targetStatus = NetworkDetailsStatus.InProgress;

            var soaStatusWithCountList = new List<SoaStatusWithCount>();

            model.SelectedSoaStatusOptions.ForEach(s =>
            {
                var soaStatusWithCount = new SoaStatusWithCount();
                soaStatusWithCount.SoaStatus = s;
                soaStatusWithCount.Count = model.SoaStatusCounts.ContainsKey(s) ? model.SoaStatusCounts[s] : null;
                soaStatusWithCountList.Add(soaStatusWithCount);
            });

            var allSoaAllStatusesCount = soaStatusWithCountList.Where(s => s.Count != null).Sum(s => s.Count ?? 0);

            ModelState.Remove("TotalCountCheck");
            if (allSoaAllStatusesCount > totalCountFromElement)
            {                
                ModelState.AddModelError("TotalCountCheck", $"The total count of all SoA statuses cannot exceed {totalCountFromElement}");
            }

            foreach (var selectedId in model.SelectedSoaStatusOptions)
            {                
                if ((!model.SoaStatusCounts.TryGetValue(selectedId, out var count) || count == null || count <= 0))
                {                    
                    ModelState.Remove($"SoaStatusCounts.{selectedId}");
                    ModelState.AddModelError($"SoaStatusCounts[{selectedId}]", $"Enter number of connections.");
                }
            }            

            if (!ModelState.IsValid)
            {
                model.SoaStatusOptions = ElementSoaHelper.GetSoaStatuses();
                return View(model);
            }

            
            
            var request = new ElementSoaStatusUpdateRequest(hnId: hnId!, stage: model.SoaStage, elementType: model?.ElementType, soaStatuses: soaStatusWithCountList, soaStatusUpdatedBy: userId, elementSoaStatus: targetStatus, soaPhase: model.SoaPhase, elementDisplayName: model.ElementName);
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
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
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
            
            this.ShowBackButton("AssessorOnboarding", "ElementSoa");
            ViewBag.SelectedAssessor = selectedAssessor;
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;

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
                foreach (var item in selectedNetworkElements.ElementsGroup!)
                {
                    var assignedAssessor = item.SoaStages?
                        .Select(s => s.Assessor)
                        .FirstOrDefault(a => a != null);

                    model.ElementOptions?.Add(new AssessorSelectElementsOption
                    {
                        //Label = item.NetworkElementInstanceName!,
                        //ElementId = item.ElementId!,
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
            
            this.ShowBackButton("AssessorOnboarding", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var modelFromSession = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            if (!ModelState.IsValid)
            {

                return View("AssessorSelectElements", modelFromSession);
            }

            foreach (var item in model.SelectedElementIds!)
            {
                foreach (var option in modelFromSession?.ElementOptions!)
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
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
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
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
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
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
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
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
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
