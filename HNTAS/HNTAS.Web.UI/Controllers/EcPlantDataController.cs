using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.EcPlantData;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddNetworkElements)]
    public class EcPlantDataController : Controller
    {
        private readonly ILogger<EcPlantDataController> _logger;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IEnergyCentrePlantDataService _energyCentrePlantDataService;
        private readonly ISessionHelper _sessionHelper;
        public EcPlantDataController(ILogger<EcPlantDataController> logger, IHeatNetworkService heatNetworkService, IEnergyCentrePlantDataService energyCentrePlantDataService, ISessionHelper sessionHelper)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _energyCentrePlantDataService = energyCentrePlantDataService;
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        public async Task<IActionResult> EcSupplyingNewBuildings()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;

            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            var networkElements = heatNetworkData?.NetworkElements;
            var ecElements = networkElements?.Elements?.Where(ne => ne.ElementType == ElementTypeInShort.EC).ToList();

            return View(ecElements);
        }

        [HttpGet]
        public async Task<IActionResult> EcPlantOptions([FromQuery] string elementId, [FromQuery] string elementName)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            var model = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);

            if (model != null)
            {                
                return View(model);
            }
            model = new EcPlantOptionsViewModel();

            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EcPlantOptions(EcPlantOptionsViewModel model)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            return RedirectToAction("EcSupplyingNewBuildings");
        }
    }
}
