using DocumentFormat.OpenXml.EMMA;
using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.ElementSoa;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetworkDetail)]
    public class ExistingElementSoaController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly ISoaService _soaProjectService;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IAssessorApi _assessorApi;
        private readonly ILogger<ExistingElementSoaController> _logger;

        public ExistingElementSoaController(ISessionHelper sessionHelper,
            ISoaService soaProjectService,
            ILogger<ExistingElementSoaController> logger,
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
            return RedirectToAction("SoaMilestones");
        }
        [HttpGet]
        public async Task<IActionResult> SoaMilestones()
        {
            this.ShowBackButton("UnderstandingSoa", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            ClearSoaAssessorSpecificSession();

            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var phase = heatNetworkData?.Phase;
            var networkType = heatNetworkData?.HeatNetworkType;
            bool hasOwnEc = heatNetworkData?.HasOwnEnergyCentre ?? false;
            var networkElements = heatNetworkData?.NetworkElements?.ElementsGroup;
            var eligibleIndex = phase == "Design" ? 1 : phase == "Construction" ? 2 : 0;
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            
            HeatNetworkType? heatNetworkType = networkType.HasValue ? (HeatNetworkType?)networkType.Value : null;
            var model = ExistingElementSoaHelper.GetElementSoaViewModel(eligibleIndex, currentStageIndex, networkElements, heatNetworkType, hasOwnEc);

            foreach (var stageInModel in model.Milestones)
            {
                if (networkElements != null)
                {
                    foreach (var elementSoaElement in networkElements)
                    {
                        if (elementSoaElement.ElementType == stageInModel.Elements?.FirstOrDefault(e => e.ElementType == elementSoaElement.ElementType)?.ElementType)
                        {
                            var modelElement = stageInModel.Elements?.Find(e => e.ElementType == elementSoaElement.ElementType);
                            var elementStage = elementSoaElement.SoaMilestones?
                                .Find(s => s.MilestoneId.HasValue && stageInModel.MilestoneId.HasValue && (int)s.MilestoneId.Value == (int)stageInModel.MilestoneId.Value);

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
                                    modelElement.SoaStatuses = elementStage.SoaStatuses.Cast<SoaStatusWithCount>().ToList();
                                }
                                modelElement.SoaStatusUpdatedAt = elementStage?.SoaStatusUpdatedAt.HasValue == true
                                    ? elementStage.SoaStatusUpdatedAt.Value.DateTime
                                    : (DateTime?)null;

                                modelElement.AssessorDetails = new List<AssessorDetails>();
                                if (elementStage?.Assessors != null)
                                {
                                    foreach (var assessor in elementStage.Assessors.Cast<SoaAssessor>().ToList())
                                    {
                                        var assessorDetails = new AssessorDetails
                                        {
                                            FirstName = assessor.FirstName!,
                                            LastName = assessor.LastName!,
                                            Email = assessor.Email!,
                                            Assessment = assessor.Assessment,
                                        };
                                        modelElement.AssessorDetails.Add(assessorDetails);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            return View("SoaMilestones", model);
        }

        [HttpGet]
        public async Task<IActionResult> SoaUpdateStatus([FromQuery] Milestone milestone, [FromQuery] ElementTypeInShort elementType, [FromQuery] HeatNetworkElementType elementDisplayType)
        {
            var tempDataKey = $"Soa_{elementType}_{milestone}";
            string soaPhase = TempData[$"{tempDataKey}_Phase"] as string;
            string elementName = TempData[$"{tempDataKey}_Element"] as string;
            var currentStageIndex = ExistingElementSoaHelper.GetMilestoneIndex(milestone);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey, currentStageIndex);
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaMilestones", "ElementSoa", targetFragment);

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            // Set element-specific ViewBag properties
            var content = ElementSoaHelper.GetSoaElementContent(elementDisplayType);
            ViewBag.Heading = content;
            var selectedNetworkElement = heatNetworkData?.NetworkElements?.ElementsGroup?.Where(e => e.ElementType == elementType).FirstOrDefault();

            var model = new ExistingElementSoaUpdateStatusViewModel();
            model.Milestone = milestone;
            model.ElementType = elementType;
            model.ElementName = elementName;
            model.SoaPhase = soaPhase;
            model.ElementDisplayType = elementDisplayType;
            model.ElementCount = selectedNetworkElement?.Count;
            model.SoaStatusOptions = ElementSoaHelper.GetSoaStatuses();

            var soaStatusesForStage = selectedNetworkElement?.SoaMilestones?
                .Where(s => s.MilestoneId.HasValue && (Milestone)s.MilestoneId.Value == milestone)
                .SelectMany(s => s.SoaStatuses!.Cast<SoaStatusWithCount>().ToList()!)
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
        public async Task<IActionResult> SoaUpdateStatus(ExistingElementSoaUpdateStatusViewModel model)
        {
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaMilestones", "ElementSoa", targetFragment);
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

            if (allSoaAllStatusesCount > totalCountFromElement)
            {
                ModelState.AddModelError($"soastatuscounts[{model.SelectedSoaStatusOptions.FirstOrDefault().ToString()}]", $"The total count of all SoA statuses cannot exceed {totalCountFromElement}");
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

            var request = new ElementSoaStatusUpdateRequestForExistingNetwork(hnId: hnId!, milestone: model.Milestone, elementType: model?.ElementType, soaStatuses: soaStatusWithCountList, soaStatusUpdatedBy: userId, elementSoaStatus: targetStatus, soaPhase: model.SoaPhase, elementDisplayName: model.ElementName);
            await _soaProjectService.UpdateElementSoaStatusForExistingNetwork(request);
            ClearSoaStatusUpdateSpecificSession();
            return RedirectToAction("SoaMilestones", "ElementSoa", targetFragment);
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult PrepareAssessorOnboarding([FromQuery] Milestone milestone, [FromQuery] string stageTitle, [FromQuery] ElementTypeInShort elementType)
        {
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding, milestone);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SoaStageTitleOfAssessorOnboarding, stageTitle);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SoaElementTypeOfAssessorOnboarding, elementType);

            return RedirectToAction("AssessorOnboarding", "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessorOnboarding()
        {
            var stage = _sessionHelper.GetFromSession<Milestone>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            ViewBag.StageTitle = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.SoaStageTitleOfAssessorOnboarding);
            var selectedAssessor = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.DefaultSelectedAssessor);
            int currentStageIndex;
            if (stage == 0)
            {
                currentStageIndex = _sessionHelper.GetFromSession<int>(HttpContext, SessionKeys.CurrentStageIndexSessionKey);
            }
            else
            {
                currentStageIndex = ExistingElementSoaHelper.GetMilestoneIndex(stage);
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
            this.ShowBackButton("SoaMilestones", "ElementSoa", targetFragment);
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
            ViewBag.StageTitle = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.SoaStageTitleOfAssessorOnboarding);
            var selectedAssessor = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.DefaultSelectedAssessor);
            var currentStageIndex = _sessionHelper.GetFromSession<int?>(HttpContext, SessionKeys.CurrentStageIndexSessionKey) ?? 0;
            var targetFragment = string.Concat("stage-", currentStageIndex);
            this.ShowBackButton("SoaMilestones", "ElementSoa", targetFragment);
            if (fullNameFromInput == null)
            {
                var stage = _sessionHelper.GetFromSession<Milestone?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
                ModelState.Remove("emailId");
                ModelState.Remove("firstName");
                ModelState.Remove("lastName");
                ModelState.Remove("fullNameFromInput");
                ModelState.Remove("assessor-autocomplete");
                ModelState.AddModelError("assessor-autocomplete", "Please select an assessor before continuing.");
                return View("AssessorOnboarding", "ElementSoa");
            }
            var assessorSearchResults = _sessionHelper.GetFromSession<List<AssessorSearchResult>>(HttpContext, SessionKeys.AssessorSearchResultsSessionKey);
            if (selectedAssessor?.FullNameWithEmail?.ToLower() != fullNameFromInput?.ToLower())
            {

                var isCorrectAssessor = assessorSearchResults?.Exists(a => a.FullNameWithEmail?.ToLower() == fullNameFromInput?.ToLower());

                if (!isCorrectAssessor ?? false)
                {
                    var stage = _sessionHelper.GetFromSession<Milestone?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
                    ModelState.Remove("emailId");
                    ModelState.Remove("firstName");
                    ModelState.Remove("lastName");
                    ModelState.Remove("fullNameFromInput");
                    ModelState.Remove("assessor-autocomplete");
                    ModelState.AddModelError("assessor-autocomplete", "Please select an assessor before continuing.");
                    return View("AssessorOnboarding", "ElementSoa");
                }
            }

            var assessorFromList = assessorSearchResults?.FirstOrDefault(a => a.FullNameWithEmail?.ToLower() == fullNameFromInput?.ToLower());

            var model = new AssessorDetails
            {
                FirstName = assessorFromList?.FirstName!,
                LastName = assessorFromList?.LastName!,
                Email = assessorFromList?.Email!
            };

            // Store in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorDetailsSessionKey, model);
            return RedirectToAction("AssessorSelectElements");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public async Task<IActionResult> AssessorSelectElementsAsync()
        {
            ClearSoaAssessorAssessmentSession();
            ViewBag.StageTitle = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.SoaStageTitleOfAssessorOnboarding);
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            var stage = _sessionHelper.GetFromSession<Milestone?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            var initiatedElementType = _sessionHelper.GetFromSession<ElementTypeInShort?>(HttpContext, SessionKeys.SoaElementTypeOfAssessorOnboarding);
            this.ShowBackButton("AssessorOnboarding", "ElementSoa");
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
                    model.ElementOptions?.Add(new AssessorSelectElementsOption
                    {
                        Label = NetworkElementHelper.GetNetworkTypeLabelForNetworkType(item.ElementDisplayType),
                        ElementType = item.ElementType!,
                        IsHidden = initiatedElementType.HasValue && item.ElementType == initiatedElementType.Value
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
            ViewBag.StageTitle = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.SoaStageTitleOfAssessorOnboarding);
            var initiatedElementType = _sessionHelper.GetFromSession<ElementTypeInShort?>(HttpContext, SessionKeys.SoaElementTypeOfAssessorOnboarding);
            if (initiatedElementType.HasValue && !model.SelectedElementIds!.Contains(initiatedElementType.Value))
            {
                model.SelectedElementIds.Insert(0, initiatedElementType.Value);
            }
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            var stage = _sessionHelper.GetFromSession<Milestone?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            var selectedAssessor = $"{assessorDetails?.FirstName} {assessorDetails?.LastName} ({assessorDetails?.Email})";

            this.ShowBackButton("AssessorOnboarding", "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var modelFromSession = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);

            foreach (var item in model.SelectedElementIds!)
            {
                foreach (var option in modelFromSession?.ElementOptions!)
                {
                    if (option.ElementType == item)
                    {
                        model.SelectedElementLabel?.Add(option.Label);
                    }
                }
            }
            model.ElementOptions = modelFromSession?.ElementOptions;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorSelectedElementSessionKey, model);

            var assessmentRoute = GetRouteActionForElement(model.SelectedElementIds!.FirstOrDefault());

            return RedirectToAction(assessmentRoute, "ElementSoa");


        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessmentSelectionEcAsync()
        {            
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, ElementTypeInShort.EC);

            this.ShowBackButton(backRoutingAction, "ElementSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;

            var model = GetAssessmentModel(ElementTypeInShort.EC, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey);

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey, model);
            return View("AssessmentSelection", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssessmentSelectionEcAsync(AssessorAssessmentSelectionViewModel model)
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            this.ShowBackButton(backRoutingAction, "ElementSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;            

            if (string.IsNullOrEmpty(model.SelectedAssessmentOption))
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), "Select which step of the assessment will they carry out for the energy centre");
                model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey);
                return View("AssessmentSelection", model);
            }

            var validationResult = await ValidateAssessment(hnId!, model.ElementType, model.SelectedAssessmentOption);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), validationResult.ErrorMessage);
                var modelFromSession = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey);
                if (modelFromSession != null)
                {
                    modelFromSession.SelectedAssessmentOption = model.SelectedAssessmentOption;
                }                
                return View("AssessmentSelection", modelFromSession);
            }

            // Store selected assessment type in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey, model);

            var nextRouteAction = GetNextRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            return RedirectToAction(nextRouteAction, "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessmentSelectionSs()
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, ElementTypeInShort.SS);

            this.ShowBackButton(backRoutingAction, "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var model = GetAssessmentModel(ElementTypeInShort.SS, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey);

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey, model);
            return View("AssessmentSelection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssessmentSelectionSsAsync(AssessorAssessmentSelectionViewModel model)
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            this.ShowBackButton(backRoutingAction, "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;            

            if (string.IsNullOrEmpty(model.SelectedAssessmentOption))
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), "Select which step of the assessment will they carry out for the substations");
                model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey);
                return View("AssessmentSelection", model);
            }

            var validationResult = await ValidateAssessment(hnId!, model.ElementType, model.SelectedAssessmentOption);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), validationResult.ErrorMessage);
                var modelFromSession = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey);
                if (modelFromSession != null)
                {
                    modelFromSession.SelectedAssessmentOption = model.SelectedAssessmentOption;
                }
                return View("AssessmentSelection", modelFromSession);
            }
            // Store selected assessment type in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey, model);

            var nextRouteAction = GetNextRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);
            return RedirectToAction(nextRouteAction, "ElementSoa");
        }        

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessmentSelectionCc()
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, ElementTypeInShort.CC);

            this.ShowBackButton(backRoutingAction, "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var model = GetAssessmentModel(ElementTypeInShort.CC, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey);

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey, model);
            return View("AssessmentSelection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssessmentSelectionCcAsync(AssessorAssessmentSelectionViewModel model)
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            this.ShowBackButton(backRoutingAction, "ElementSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;

            if (string.IsNullOrEmpty(model.SelectedAssessmentOption))
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), "Select which step of the assessment will they carry out for the consumer connection");
                model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey);
                return View("AssessmentSelection", model);
            }

            var validationResult = await ValidateAssessment(hnId!, model.ElementType, model.SelectedAssessmentOption);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), validationResult.ErrorMessage);
                var modelFromSession = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey);
                if (modelFromSession != null)
                {
                    modelFromSession.SelectedAssessmentOption = model.SelectedAssessmentOption;
                }
                return View("AssessmentSelection", modelFromSession);
            }
            // Store selected assessment type in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey, model);

            var nextRouteAction = GetNextRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            return RedirectToAction(nextRouteAction, "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessmentSelectionCdn()
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, ElementTypeInShort.CDN);

            this.ShowBackButton(backRoutingAction, "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var model = GetAssessmentModel(ElementTypeInShort.CDN, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey);

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey, model);
            return View("AssessmentSelection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssessmentSelectionCdnAsync(AssessorAssessmentSelectionViewModel model)
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            this.ShowBackButton(backRoutingAction, "ElementSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;

            if (string.IsNullOrEmpty(model.SelectedAssessmentOption))
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), "Select which step of the assessment will they carry out for the communal distribution network");
                model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey);
                return View("AssessmentSelection", model);
            }

            var validationResult = await ValidateAssessment(hnId!, model.ElementType, model.SelectedAssessmentOption);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), validationResult.ErrorMessage);
                var modelFromSession = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey);
                if (modelFromSession != null)
                {
                    modelFromSession.SelectedAssessmentOption = model.SelectedAssessmentOption;
                }
                return View("AssessmentSelection", modelFromSession);
            }

            // Store selected assessment type in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey, model);

            var nextRouteAction = GetNextRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            return RedirectToAction(nextRouteAction, "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public IActionResult AssessmentSelectionDdn()
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, ElementTypeInShort.DDN);

            this.ShowBackButton(backRoutingAction, "ElementSoa");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var model = GetAssessmentModel(ElementTypeInShort.DDN, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey);

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey, model);
            return View("AssessmentSelection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssessmentSelectionDdnAsync(AssessorAssessmentSelectionViewModel model)
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetBackRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            this.ShowBackButton(backRoutingAction, "ElementSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;

            if (string.IsNullOrEmpty(model.SelectedAssessmentOption))
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), "Select which step of the assessment will they carry out for the district distribution network");
                model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey);
                return View("AssessmentSelection", model);
            }

            var validationResult = await ValidateAssessment(hnId!, model.ElementType, model.SelectedAssessmentOption);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(nameof(model.SelectedAssessmentOption), validationResult.ErrorMessage);
                var modelFromSession = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey);
                if (modelFromSession != null)
                {
                    modelFromSession.SelectedAssessmentOption = model.SelectedAssessmentOption;
                }
                return View("AssessmentSelection", modelFromSession);
            }

            // Store selected assessment type in session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey, model);

            var nextRouteAction = GetNextRouteAction(selectedElements!.SelectedElementIds!, model.ElementType);

            return RedirectToAction(nextRouteAction, "ElementSoa");
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAssignAssessor)]
        [HttpGet]
        public async Task<IActionResult> AssessorElementSelectionOverviewAsync()
        {
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetRouteActionForElement(selectedElements.SelectedElementIds.LastOrDefault());

            this.ShowBackButton(backRoutingAction, "ElementSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var phase = heatNetworkData?.Phase;
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);

            var ecData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey);
            var ssData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey);
            var ccData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey);
            var cdnData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey);
            var ddnData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey);

            var assessorAssessments = new List<AssessorAssessmentSelectionViewModel>();
            foreach (var selectedElement in selectedElements.SelectedElementIds!)
            {
                switch (selectedElement)
                {
                    case ElementTypeInShort.EC:
                        if (ecData != null)
                        {
                            assessorAssessments.Add(ecData);
                        }
                        break;
                    case ElementTypeInShort.SS:
                        if (ssData != null)
                        {
                            assessorAssessments.Add(ssData);
                        }
                        break;
                    case ElementTypeInShort.CC:
                        if (ccData != null)
                        {
                            assessorAssessments.Add(ccData);
                        }
                        break;
                    case ElementTypeInShort.CDN:
                        if (cdnData != null)
                        {
                            assessorAssessments.Add(cdnData);
                        }
                        break;
                    case ElementTypeInShort.DDN:
                        if (ddnData != null)
                        {
                            assessorAssessments.Add(ddnData);
                        }
                        break;
                }
            }


            var model = new AssessorElementSelectionOverviewModel
            {
                AssessorAssessments = assessorAssessments!,
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
            var selectedElements = _sessionHelper.GetFromSession<AssessorSelectElementsViewModel>(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            var backRoutingAction = GetRouteActionForElement(selectedElements.SelectedElementIds.LastOrDefault());

            this.ShowBackButton(backRoutingAction, "ElementSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);

            var stage = _sessionHelper.GetFromSession<Milestone?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);

            var ecData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey);
            var ssData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey);
            var ccData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey);
            var cdnData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey);
            var ddnData = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey);

            var assessorAssessmentForElements = new List<AssessorAssessmentForElement>();
            GetAssessorAssessmentForElements(ecData, assessorDetails, assessorAssessmentForElements);
            GetAssessorAssessmentForElements(ssData, assessorDetails, assessorAssessmentForElements);
            GetAssessorAssessmentForElements(ccData, assessorDetails, assessorAssessmentForElements);
            GetAssessorAssessmentForElements(cdnData, assessorDetails, assessorAssessmentForElements);
            GetAssessorAssessmentForElements(ddnData, assessorDetails, assessorAssessmentForElements);


            var requestModel = new ElementSoaAssignAssessorRequestForExistingNetwork
            {
                HnId = hnId!,
                AssessorAssessmentForElements = assessorAssessmentForElements,
                Milestone = stage,
                UpdatedBy = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey)

            };
            await _soaProjectService.AssignAssessorForExistingNetwork(requestModel);
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
            return RedirectToAction("SoaMilestones", "ElementSoa", targetFragment);
        }

        private string GetRouteActionForElement(ElementTypeInShort elementType)
        {
            if (elementType == ElementTypeInShort.EC)
            {
                return "AssessmentSelectionEc";
            }
            else if (elementType == ElementTypeInShort.SS)
            {
                return "AssessmentSelectionSs";
            }
            else if (elementType == ElementTypeInShort.DDN)
            {
                return "AssessmentSelectionDdn";
            }
            else if (elementType == ElementTypeInShort.CDN)
            {
                return "AssessmentSelectionCdn";
            }
            else if (elementType == ElementTypeInShort.CC)
            {
                return "AssessmentSelectionCc";
            }
            else
            {
                return "";
            }
        }

        private string GetBackRouteAction(List<ElementTypeInShort> selectedElementIds, ElementTypeInShort currentElement)
        {
            ElementTypeInShort previousElement = 0;
            for (var i = 0; i < selectedElementIds.Count; i++)
            {
                if (selectedElementIds[i] == currentElement && i - 1 >= 0)
                {
                    previousElement = selectedElementIds[i - 1];
                    break;
                }
            }
            if (previousElement == ElementTypeInShort.EC)
            {
                return "AssessmentSelectionEc";
            }
            else if (previousElement == ElementTypeInShort.SS)
            {
                return "AssessmentSelectionSs";
            }
            else if (previousElement == ElementTypeInShort.DDN)
            {
                return "AssessmentSelectionDdn";
            }
            else if (previousElement == ElementTypeInShort.CDN)
            {
                return "AssessmentSelectionCdn";
            }
            else if (previousElement == ElementTypeInShort.CC)
            {
                return "AssessmentSelectionCc";
            }
            else
            {
                return "AssessorSelectElements";
            }
        }

        private string GetNextRouteAction(List<ElementTypeInShort> selectedElementIds, ElementTypeInShort currentElement)
        {
            ElementTypeInShort nextElement = 0;
            for (var i = 0; i < selectedElementIds.Count; i++)
            {
                if (selectedElementIds[i] == currentElement && i + 1 < selectedElementIds.Count)
                {
                    nextElement = selectedElementIds[i + 1];
                    break;
                }
            }
            if (nextElement == ElementTypeInShort.EC)
            {
                return "AssessmentSelectionEc";
            }
            else if (nextElement == ElementTypeInShort.SS)
            {
                return "AssessmentSelectionSs";
            }
            else if (nextElement == ElementTypeInShort.DDN)
            {
                return "AssessmentSelectionDdn";
            }
            else if (nextElement == ElementTypeInShort.CDN)
            {
                return "AssessmentSelectionCdn";
            }
            else if (nextElement == ElementTypeInShort.CC)
            {
                return "AssessmentSelectionCc";
            }
            else
            {
                return "AssessorElementSelectionOverview";
            }
        }

        private AssessorAssessmentSelectionViewModel GetAssessmentModel(ElementTypeInShort elementType, string sessionKey)
        {
            var model = _sessionHelper.GetFromSession<AssessorAssessmentSelectionViewModel>(HttpContext, sessionKey) ?? new AssessorAssessmentSelectionViewModel();
            model.ElementType = elementType;
            model.AssessmentOptions = ElementSoaHelper.GetAssessmentOptions();        

            return model;
        }

        private void GetAssessorAssessmentForElements(AssessorAssessmentSelectionViewModel assessorAssessmentdata, AssessorDetails assessorDetails, List<AssessorAssessmentForElement> assessorAssessmentForElements)
        {
            var assessorAssessmentForElement = new AssessorAssessmentForElement();

            if (assessorAssessmentdata != null)
            {
                assessorAssessmentForElement.ElementType = assessorAssessmentdata.ElementType;
                assessorAssessmentForElement.AssessorAssessments = new List<AssessorAssessment>()
                {
                    new AssessorAssessment
                    {
                        AssessorEmail = assessorDetails?.Email,
                        AssessorFirstName = assessorDetails?.FirstName,
                        AssessorLastName = assessorDetails?.LastName,
                        Assessment = assessorAssessmentdata.SelectedAssessmentOption
                    }
                };
                assessorAssessmentForElements.Add(assessorAssessmentForElement);
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateAssessment(string hnId, ElementTypeInShort selectedElementType, string selectedAssessmentOption)
        {
            var assessorDetails = _sessionHelper.GetFromSession<AssessorDetails>(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            var stage = _sessionHelper.GetFromSession<Milestone?>(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var elementGroup = heatNetworkData?.NetworkElements?.ElementsGroup.Where(a => a.ElementType == selectedElementType).FirstOrDefault();
            if (elementGroup != null)
            {
                var stageForElement = elementGroup.SoaMilestones?.Where(s => s.MilestoneId.HasValue && (Milestone)s.MilestoneId.Value == stage).FirstOrDefault();
                if (stageForElement != null)
                {
                    var assessorsForStageAndElement = stageForElement.Assessors!.Cast<SoaAssessor>().ToList();


                    if (assessorsForStageAndElement != null && assessorsForStageAndElement.Count > 0)
                    {
                        var assessorForStageAndElement = assessorsForStageAndElement.Where(a => a.Email == assessorDetails?.Email).FirstOrDefault();
                        // If same assessor already has an assessment
                        if (assessorForStageAndElement != null)
                        {
                            return (false, $"This assessor has already been assigned to {assessorForStageAndElement.Assessment}. Choose a different assessor.");
                        }

                        var assessmentForExistingAssessors = assessorsForStageAndElement.Select(a => a.Assessment).ToList();
                        // if the assessment option is already assigned to another assessor for the same stage and element
                        if (assessmentForExistingAssessors.Contains(selectedAssessmentOption))
                        {
                            return (false, "This assessment option has already been assigned to another assessor for this stage. Choose a different assessment option.");
                        }

                        // if the selected assessment option is 'Review' or 'Decision', and there is already an assessor assigned to  'Review and Decision' for the same stage and element
                        if ((selectedAssessmentOption == AssessmentConstants.Review
                            || selectedAssessmentOption == AssessmentConstants.Decision)
                            && assessmentForExistingAssessors.Contains(AssessmentConstants.ReviewAndDecision))
                        {
                            return (false, "This assessment option has already been assigned to another assessor for this stage. Choose a different assessment option.");
                        }

                        // if the selected assessment option is 'Review and Decision', and there is already an assessor assigned to  'Review' or 'Decision' for the same stage and element
                        if (selectedAssessmentOption == AssessmentConstants.ReviewAndDecision
                            && (assessmentForExistingAssessors.Contains(AssessmentConstants.Review)
                            || assessmentForExistingAssessors.Contains(AssessmentConstants.Decision)))
                        {
                            return (false, "This assessment option has already been assigned to another assessor for this stage. Choose a different assessment option.");
                        }
                    }

                }
            }
            return (true, string.Empty);
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
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorElementSelectionOverviewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.DefaultSelectedAssessor);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorSearchResultsSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SoaElementTypeOfAssessorOnboarding);
            ClearSoaAssessorAssessmentSession();
        }

        private void ClearSoaAssessorAssessmentSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey);
        }
    }
}
