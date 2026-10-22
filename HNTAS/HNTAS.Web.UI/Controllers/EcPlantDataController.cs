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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectEcPlantOptions([FromForm] string elementId, [FromForm] string elementName)
        {
            TempData["EcElementId"] = elementId;
            TempData["EcElementName"] = elementName;

            return RedirectToAction(nameof(EcPlantOptions));
        }

        [HttpGet]
        public async Task<IActionResult> EcPlantOptions()
        {
            this.ShowBackButton("EcSupplyingNewBuildings", "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

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
            this.ShowBackButton("EcSupplyingNewBuildings", "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if ((model.NumberOfChp.GetValueOrDefault() + model.NumberOfHeatPump.GetValueOrDefault() + model.NumberOfHighHeatRecoverySources.GetValueOrDefault() + model.NumberOfBoilerPlants.GetValueOrDefault()) <= 0)
            {
                ModelState.AddModelError(nameof(model.NumberOfChp), "Enter a whole number of 0 or more for number of combined heat and power systems");
                ModelState.AddModelError(nameof(model.NumberOfHeatPump), "Enter a whole number of 0 or more for number of heat pump systems");
                ModelState.AddModelError(nameof(model.NumberOfHighHeatRecoverySources), "Enter a whole number of 0 or more for number of heat recovered without heat pumps");
                ModelState.AddModelError(nameof(model.NumberOfBoilerPlants), "Enter a whole number of 0 or more for number of boiler plants");
                return View(model);
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey, model);
            return RedirectToAction("chpInformation");
        }

        [HttpGet]
        public async Task<IActionResult> ChpInformation()
        {
            this.ShowBackButton("EcPlantOptions", "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypesTask = _energyCentrePlantDataService.GetFuelTypes();
            var operationalModesTask = _energyCentrePlantDataService.GetChpOperationalModes();
            await Task.WhenAll(fuelTypesTask, operationalModesTask);
            var fuelTypes = fuelTypesTask.Result;
            var operationalModes = operationalModesTask.Result;

            var plantOptionsModel = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);

            var model = _sessionHelper.GetFromSession<ChpInformationViewModel>(HttpContext, SessionKeys.ChpInformationViewModelSessionKey);
            if (model != null)
            {
                model.FuelTypes = fuelTypes;
                model.OperationalModes = operationalModes;
                return View(model);
            }
            model = new ChpInformationViewModel();
            if (plantOptionsModel != null && plantOptionsModel.NumberOfChp.HasValue)
            {
                model.FuelTypes = fuelTypes;
                model.OperationalModes = operationalModes;
                for (int i = 0; i < plantOptionsModel.NumberOfChp.Value; i++)
                {
                    model.ChpInformationList.Add(new ChpInformation
                    {
                        
                    });
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChpInformation(ChpInformationViewModel model)
        {
            this.ShowBackButton("EcPlantOptions", "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypesTask = _energyCentrePlantDataService.GetFuelTypes();
            var operationalModesTask = _energyCentrePlantDataService.GetChpOperationalModes();
            await Task.WhenAll(fuelTypesTask, operationalModesTask);
            model.FuelTypes = fuelTypesTask.Result;
            model.OperationalModes = operationalModesTask.Result;

            foreach (var chpInfo in model.ChpInformationList)
            {
                if (!chpInfo.MaximumHeatOutput.HasValue)
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].MaximumHeatOutput", $"Enter the maximum heat output for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
                }
                if (!chpInfo.MaximumElectricOutput.HasValue)
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].MaximumElectricOutput", $"Enter the maximum electric output for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
                }
                if (!chpInfo.FuelOrHeatSourceValue.HasValue)
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].FuelOrHeatSourceValue", $"Enter the fuel/heat source for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
                }
                if (string.IsNullOrWhiteSpace(chpInfo.OperationalModeValue))
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].OperationalModeValue", $"Enter the operational mode for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
                }
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ChpInformationViewModelSessionKey, model);
            return RedirectToAction("HeatPumpInformation", "EcPlantData");
        }

        [HttpGet]
        public async Task<IActionResult> HeatPumpInformation()
        {
            this.ShowBackButton("EcPlantOptions", "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();
            var plantOptionsModel = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);

            var model = _sessionHelper.GetFromSession<HeatPumpInformationViewModel>(HttpContext, SessionKeys.HeatPumpInformationViewModelSessionKey);
            if (model != null)
            {
                model.FuelTypes = fuelTypes;                
                return View(model);
            }
            model = new HeatPumpInformationViewModel();
            if (plantOptionsModel != null && plantOptionsModel.NumberOfHeatPump.HasValue)
            {
                model.FuelTypes = fuelTypes;                
                for (int i = 0; i < plantOptionsModel.NumberOfHeatPump.Value; i++)
                {
                    model.HeatPumpInformationList.Add(new HeatPumpInformation
                    {
                    });
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HeatPumpInformation(HeatPumpInformationViewModel model)
        {
            this.ShowBackButton("EcPlantOptions", "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();           

            foreach (var heatPumpInfo in model.HeatPumpInformationList)
            {
                if (!heatPumpInfo.MaximumHeatOutput.HasValue)
                {
                    ModelState.AddModelError($"HeatPumpInformationList[{model.HeatPumpInformationList.IndexOf(heatPumpInfo)}].MaximumHeatOutput", $"Enter the maximum heat output for Heat Pump system {model.HeatPumpInformationList.IndexOf(heatPumpInfo) + 1}");
                }
                if (!heatPumpInfo.FuelOrHeatSourceValue.HasValue)
                {
                    ModelState.AddModelError($"HeatPumpInformationList[{model.HeatPumpInformationList.IndexOf(heatPumpInfo)}].FuelOrHeatSourceValue", $"Enter the fuel/heat source for Heat Pump system {model.HeatPumpInformationList.IndexOf(heatPumpInfo) + 1}");
                }                
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatPumpInformationViewModelSessionKey, model);
            return RedirectToAction("HeatPumpInformation", "EcPlantData");
        }
    }
}
