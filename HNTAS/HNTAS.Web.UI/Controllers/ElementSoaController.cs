using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.ElementSoa;
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
        private readonly ILogger<ElementSoaController> _logger;

        public ElementSoaController(ISessionHelper sessionHelper,
            ISoaService soaProjectService,
            ILogger<ElementSoaController> logger,
            IHeatNetworkService heatNetworkService)
        {
            _sessionHelper = sessionHelper;
            _soaProjectService = soaProjectService;
            _logger = logger;
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
            var request = new ElementSoaStatusUpdateRequest(hnId: hnId!, stage: modelFromSession?.SoaStage, elementId: model?.ElementId, soaStatus: model?.SelectedSoaStatus, soaStatusUpdatedBy: userId, elementSoaStatus: targetStatus);
            await _soaProjectService.UpdateElementSoaStatus(request);
            ClearSoaSpecificSession();
            return RedirectToAction("SoaStages", "ElementSoa", targetFragment);
        }

        private void ClearSoaSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.CurrentStageIndexSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ElementSoaStatusUpdateModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ElementSoaIncompleteSoaSessionKey);
        }
    }
}
