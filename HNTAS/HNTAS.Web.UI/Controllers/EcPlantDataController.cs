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
            ClearPlantDataSpecificSession();
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);
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
            ClearPlantDataSpecificSession();
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
            var nextAction = ActionList().FirstOrDefault();
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public async Task<IActionResult> ChpInformation()
        {
            this.ShowBackButton(BackAction("ChpInformation"), "EcPlantData");
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

            ViewBag.FuelTypes = fuelTypes;
            ViewBag.OperationalModes = operationalModes;

            var plantOptionsModel = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);

            var model = _sessionHelper.GetFromSession<ChpInformationViewModel>(HttpContext, SessionKeys.ChpInformationViewModelSessionKey);
            if (model != null)
            {                
                return View(model);
            }
            model = new ChpInformationViewModel();
            if (plantOptionsModel != null && plantOptionsModel.NumberOfChp.HasValue)
            {                
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
            this.ShowBackButton(BackAction("ChpInformation"), "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypesTask = _energyCentrePlantDataService.GetFuelTypes();
            var operationalModesTask = _energyCentrePlantDataService.GetChpOperationalModes();
            await Task.WhenAll(fuelTypesTask, operationalModesTask);            
            ViewBag.FuelTypes = fuelTypesTask.Result;
            ViewBag.OperationalModes = operationalModesTask.Result;

            foreach (var chpInfo in model.ChpInformationList)
            {
                if (!chpInfo.MaximumHeatOutput.HasValue)
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].MaximumHeatOutput", $"Enter the maximum heat output for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
                }
                else if (chpInfo.MaximumHeatOutput < 0 || chpInfo.MaximumHeatOutput > 9999)
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].MaximumHeatOutput", $"Enter a whole number of 0 or more for the maximum heat output for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
                }
                if (!chpInfo.MaximumElectricOutput.HasValue)
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].MaximumElectricOutput", $"Enter the maximum electric output for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
                }
                else if (chpInfo.MaximumElectricOutput < 0 || chpInfo.MaximumElectricOutput > 9999)
                {
                    ModelState.AddModelError($"ChpInformationList[{model.ChpInformationList.IndexOf(chpInfo)}].MaximumElectricOutput", $"Enter a whole number of 0 or more for the maximum electric output for Combined heat and power system {model.ChpInformationList.IndexOf(chpInfo) + 1}");
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

            return RedirectToAction(NextAction("ChpInformation"), "EcPlantData");
        }

        [HttpGet]
        public async Task<IActionResult> HeatPumpInformation()
        {
            this.ShowBackButton(BackAction("HeatPumpInformation"), "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();
            ViewBag.FuelTypes = fuelTypes;
            var plantOptionsModel = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);

            var model = _sessionHelper.GetFromSession<HeatPumpInformationViewModel>(HttpContext, SessionKeys.HeatPumpInformationViewModelSessionKey);
            if (model != null)
            {                                
                return View(model);
            }
            model = new HeatPumpInformationViewModel();
            if (plantOptionsModel != null && plantOptionsModel.NumberOfHeatPump.HasValue)
            {
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
            this.ShowBackButton(BackAction("HeatPumpInformation"), "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();           
            ViewBag.FuelTypes = fuelTypes;
            foreach (var heatPumpInfo in model.HeatPumpInformationList)
            {
                if (!heatPumpInfo.MaximumHeatOutput.HasValue)
                {
                    ModelState.AddModelError($"HeatPumpInformationList[{model.HeatPumpInformationList.IndexOf(heatPumpInfo)}].MaximumHeatOutput", $"Enter the maximum heat output for Heat Pump system {model.HeatPumpInformationList.IndexOf(heatPumpInfo) + 1}");
                }
                else if (heatPumpInfo.MaximumHeatOutput < 0 || heatPumpInfo.MaximumHeatOutput > 9999)
                {
                    ModelState.AddModelError($"HeatPumpInformationList[{model.HeatPumpInformationList.IndexOf(heatPumpInfo)}].MaximumHeatOutput", $"Enter a whole number of 0 or more for the maximum heat output for Heat Pump system {model.HeatPumpInformationList.IndexOf(heatPumpInfo) + 1}");
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
            return RedirectToAction(NextAction("HeatPumpInformation"), "EcPlantData");
        }

        [HttpGet]
        public async Task<IActionResult> HighHeatRecoveryInformation()
        {
            this.ShowBackButton(BackAction("HighHeatRecoveryInformation"), "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();
            ViewBag.FuelTypes = fuelTypes;
            var plantOptionsModel = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);

            var model = _sessionHelper.GetFromSession<HighHeatRecoveryInformationViewModel>(HttpContext, SessionKeys.HighHeatRecoveryInformationViewModelSessionKey);
            if (model != null)
            {
                return View(model);
            }
            model = new HighHeatRecoveryInformationViewModel();
            if (plantOptionsModel != null && plantOptionsModel.NumberOfHighHeatRecoverySources.HasValue)
            {
                for (int i = 0; i < plantOptionsModel.NumberOfHighHeatRecoverySources.Value; i++)
                {
                    model.HighHeatRecoveryInformationList.Add(new HighHeatRecoveryInformation
                    {
                    });
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HighHeatRecoveryInformation(HighHeatRecoveryInformationViewModel model)
        {
            this.ShowBackButton(BackAction("HighHeatRecoveryInformation"), "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();
            ViewBag.FuelTypes = fuelTypes;
            foreach (var highHeatRecoveryInfo in model.HighHeatRecoveryInformationList)
            {
                if (!highHeatRecoveryInfo.MaximumHeatOutput.HasValue)
                {
                    ModelState.AddModelError($"HighHeatRecoveryInformationList[{model.HighHeatRecoveryInformationList.IndexOf(highHeatRecoveryInfo)}].MaximumHeatOutput", $"Enter the maximum heat output for High Heat Recovery system {model.HighHeatRecoveryInformationList.IndexOf(highHeatRecoveryInfo) + 1}");
                }
                else if (highHeatRecoveryInfo.MaximumHeatOutput < 0 || highHeatRecoveryInfo.MaximumHeatOutput > 9999)
                {
                    ModelState.AddModelError($"HighHeatRecoveryInformationList[{model.HighHeatRecoveryInformationList.IndexOf(highHeatRecoveryInfo)}].MaximumHeatOutput", $"Enter a whole number of 0 or more for the maximum heat output for High Heat Recovery system {model.HighHeatRecoveryInformationList.IndexOf(highHeatRecoveryInfo) + 1}");
                }
                if (!highHeatRecoveryInfo.FuelOrHeatSourceValue.HasValue)
                {
                    ModelState.AddModelError($"HighHeatRecoveryInformationList[{model.HighHeatRecoveryInformationList.IndexOf(highHeatRecoveryInfo)}].FuelOrHeatSourceValue", $"Enter the fuel/heat source for High Heat Recovery system {model.HighHeatRecoveryInformationList.IndexOf(highHeatRecoveryInfo) + 1}");
                }
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HighHeatRecoveryInformationViewModelSessionKey, model);
            return RedirectToAction(NextAction("HighHeatRecoveryInformation"), "EcPlantData");
        }

        [HttpGet]
        public async Task<IActionResult> BoilerPlantInformation()
        {
            this.ShowBackButton(BackAction("BoilerPlantInformation"), "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();
            ViewBag.FuelTypes = fuelTypes;
            var plantOptionsModel = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);

            var model = _sessionHelper.GetFromSession<BoilerPlantInformationViewModel>(HttpContext, SessionKeys.BoilerPlantInformationViewModelSessionKey);
            if (model != null)
            {
                return View(model);
            }
            model = new BoilerPlantInformationViewModel();
            if (plantOptionsModel != null && plantOptionsModel.NumberOfBoilerPlants.HasValue)
            {
                for (int i = 0; i < plantOptionsModel.NumberOfBoilerPlants.Value; i++)
                {
                    model.BoilerPlantInformationList.Add(new BoilerPlantInformation
                    {
                    });
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoilerPlantInformation(BoilerPlantInformationViewModel model)
        {
            this.ShowBackButton(BackAction("BoilerPlantInformation"), "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();

            var fuelTypes = await _energyCentrePlantDataService.GetFuelTypes();
            ViewBag.FuelTypes = fuelTypes;
            foreach (var boilerPlantInfo in model.BoilerPlantInformationList)
            {
                if (!boilerPlantInfo.MaximumHeatOutput.HasValue)
                {
                    ModelState.AddModelError($"BoilerPlantInformationList[{model.BoilerPlantInformationList.IndexOf(boilerPlantInfo)}].MaximumHeatOutput", $"Enter the maximum heat output for Boiler Plant system {model.BoilerPlantInformationList.IndexOf(boilerPlantInfo) + 1}");
                }
                else if (boilerPlantInfo.MaximumHeatOutput < 0 || boilerPlantInfo.MaximumHeatOutput > 9999)
                {
                    ModelState.AddModelError($"BoilerPlantInformationList[{model.BoilerPlantInformationList.IndexOf(boilerPlantInfo)}].MaximumHeatOutput", $"Enter a whole number of 0 or more for the maximum heat output for Boiler Plant system {model.BoilerPlantInformationList.IndexOf(boilerPlantInfo) + 1}");
                }
                if (!boilerPlantInfo.FuelOrHeatSourceValue.HasValue)
                {
                    ModelState.AddModelError($"BoilerPlantInformationList[{model.BoilerPlantInformationList.IndexOf(boilerPlantInfo)}].FuelOrHeatSourceValue", $"Enter the fuel/heat source for Boiler Plant system {model.BoilerPlantInformationList.IndexOf(boilerPlantInfo) + 1}");
                }
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.BoilerPlantInformationViewModelSessionKey, model);
            return RedirectToAction(NextAction("BoilerPlantInformation"), "EcPlantData");
        }

        [HttpGet]
        public async Task<IActionResult> EcPlantSummary()
        {
            var lastAction = ActionList().LastOrDefault();            
            this.ShowBackButton(lastAction!, "EcPlantData");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            ViewBag.SelectedElementId = TempData.Peek("EcElementId")?.ToString();
            ViewBag.SelectedElementName = TempData.Peek("EcElementName")?.ToString();
            
            var chpInformationModel = _sessionHelper.GetFromSession<ChpInformationViewModel>(HttpContext, SessionKeys.ChpInformationViewModelSessionKey);
            var heatPumpInformationModel = _sessionHelper.GetFromSession<HeatPumpInformationViewModel>(HttpContext, SessionKeys.HeatPumpInformationViewModelSessionKey);
            var highHeatRecoveryInformationModel = _sessionHelper.GetFromSession<HighHeatRecoveryInformationViewModel>(HttpContext, SessionKeys.HighHeatRecoveryInformationViewModelSessionKey);
            var boilerPlantInformationModel = _sessionHelper.GetFromSession<BoilerPlantInformationViewModel>(HttpContext, SessionKeys.BoilerPlantInformationViewModelSessionKey);

            var fuelTypesTask = _energyCentrePlantDataService.GetFuelTypes();
            var operationalModesTask = _energyCentrePlantDataService.GetChpOperationalModes();
            await Task.WhenAll(fuelTypesTask, operationalModesTask);
            var fuelTypes = fuelTypesTask.Result;
            var operationalModes = operationalModesTask.Result;

            var model = new EcPlantSummaryViewModel();
            if (chpInformationModel != null && chpInformationModel.ChpInformationList != null && chpInformationModel.ChpInformationList.Count > 0)
            {   
                model.ChpInformationSummaryList = new List<ChpInformationSummary>();
                foreach (var chpInfo in chpInformationModel.ChpInformationList)
                {                   
                    var chpSummary = new ChpInformationSummary
                    {
                        MaximumHeatOutput = $"Maximum heat output (kWth) - {chpInfo.MaximumHeatOutput}",
                        MaximumElectricOutput = $"Maximum electric output (kWe) - {chpInfo.MaximumElectricOutput}",
                        FuelOrHeatSourceValue = $"Fuel or heat source - {fuelTypes?.FirstOrDefault(f => f.FuelId == chpInfo.FuelOrHeatSourceValue)?.Type}",
                        OperationalModeValue = $"Operational mode - {operationalModes?.FirstOrDefault(o => o.Ident == chpInfo.OperationalModeValue)?.OperationalMode}"
                    };
                    model.ChpInformationSummaryList.Add(chpSummary);
                }
            }

            if (heatPumpInformationModel != null && heatPumpInformationModel.HeatPumpInformationList != null && heatPumpInformationModel.HeatPumpInformationList.Count > 0)
            {
                model.HeatPumpInformationSummaryList = new List<HeatPumpInformationSummary>();
                foreach (var heatPumpInfo in heatPumpInformationModel.HeatPumpInformationList)
                {
                    var heatPumpSummary = new HeatPumpInformationSummary
                    {
                        MaximumHeatOutput = $"Maximum heat output (kWth) - {heatPumpInfo.MaximumHeatOutput}",
                        FuelOrHeatSourceValue = $"Fuel or heat source - {fuelTypes?.FirstOrDefault(f => f.FuelId == heatPumpInfo.FuelOrHeatSourceValue)?.Type}"
                    };
                    model.HeatPumpInformationSummaryList.Add(heatPumpSummary);
                }
            }

            if (highHeatRecoveryInformationModel != null && highHeatRecoveryInformationModel.HighHeatRecoveryInformationList != null && highHeatRecoveryInformationModel.HighHeatRecoveryInformationList.Count > 0)
            {
                model.HighHeatRecoveryInformationSummaryList = new List<HighHeatRecoveryInformationSummary>();
                foreach (var highHeatRecoveryInfo in highHeatRecoveryInformationModel.HighHeatRecoveryInformationList)
                {
                    var highHeatRecoverySummary = new HighHeatRecoveryInformationSummary
                    {
                        MaximumHeatOutput = $"Maximum heat output (kWth) - {highHeatRecoveryInfo.MaximumHeatOutput}",
                        FuelOrHeatSourceValue = $"Fuel or heat source - {fuelTypes?.FirstOrDefault(f => f.FuelId == highHeatRecoveryInfo.FuelOrHeatSourceValue)?.Type}"
                    };
                    model.HighHeatRecoveryInformationSummaryList.Add(highHeatRecoverySummary);
                }
            }

            if (boilerPlantInformationModel != null && boilerPlantInformationModel.BoilerPlantInformationList != null && boilerPlantInformationModel.BoilerPlantInformationList.Count > 0)
            {
                model.BoilerPlantInformationSummaryList = new List<BoilerPlantInformationSummary>();
                foreach (var boilerPlantInfo in boilerPlantInformationModel.BoilerPlantInformationList)
                {
                    var boilerPlantSummary = new BoilerPlantInformationSummary
                    {
                        MaximumHeatOutput = $"Maximum heat output (kWth) - {boilerPlantInfo.MaximumHeatOutput}",
                        FuelOrHeatSourceValue = $"Fuel or heat source - {fuelTypes?.FirstOrDefault(f => f.FuelId == boilerPlantInfo.FuelOrHeatSourceValue)?.Type}"
                    };
                    model.BoilerPlantInformationSummaryList.Add(boilerPlantSummary);
                }
            }

            return View(model);
        }

        private List<string> ActionList()
        {
            var actonList = new List<string>();
            var plantOptionsModel = _sessionHelper.GetFromSession<EcPlantOptionsViewModel>(HttpContext, SessionKeys.EcPlantOptionsViewModelSessionKey);
            if (plantOptionsModel != null)
            {
                if (plantOptionsModel.NumberOfChp.GetValueOrDefault() > 0)
                {
                    actonList.Add("ChpInformation");
                }
                if (plantOptionsModel.NumberOfHeatPump.GetValueOrDefault() > 0)
                {
                    actonList.Add("HeatPumpInformation");
                }
                if (plantOptionsModel.NumberOfHighHeatRecoverySources.GetValueOrDefault() > 0)
                {
                    actonList.Add("HighHeatRecoveryInformation");
                }
                if (plantOptionsModel.NumberOfBoilerPlants.GetValueOrDefault() > 0)
                {
                    actonList.Add("BoilerPlantInformation");
                }
            }
            return actonList;
        }

        private string BackAction(string currentAction)
        {
            var actionList = ActionList();
            var currentIndex = actionList.IndexOf(currentAction);
            var backAction = currentIndex > 0 ? actionList[currentIndex - 1] : "EcPlantOptions";
            return backAction;
        }
        private string NextAction(string currentAction)
        {
            var actionList = ActionList();
            var currentIndex = actionList.IndexOf(currentAction);
            var nextAction = currentIndex < actionList.Count - 1 ? actionList[currentIndex + 1] : "EcPlantSummary";
            return nextAction;
        }

        private void ClearPlantDataSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ChpInformationViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.HeatPumpInformationViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.HighHeatRecoveryInformationViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.BoilerPlantInformationViewModelSessionKey);
        }
    }
}
