using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.NetworkCharacteristics;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using ApiHeatNetworkType = HNTAS.Api.Client.Model.HeatNetworkType;

namespace HNTAS.Web.UI.Controllers
{
    public class NetworkCharacteristicsController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IHeatNetworkService _heatNetworkService;
        public NetworkCharacteristicsController(ISessionHelper sessionHelper, IHeatNetworkService heatNetworkService)
        {
            _sessionHelper = sessionHelper;
            _heatNetworkService = heatNetworkService;
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkTypeAsync()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            TempData["HnId"] = hnId;
            
            var model = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey);
            if (model == null)
            {
                var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                var heatNetworkCharacteristics = heatNetworkData?.NetworkCharacteristics;
                model = new HeatNetworkTypeViewModel { HeatNetworkTypes = Utility.GetHeatNetworkTypeOptions() };
                
                if (heatNetworkCharacteristics != null )
                {
                    model.SelectedHeatNetworkType = heatNetworkCharacteristics.HeatNetworkType switch
                    {
                        ApiHeatNetworkType.NetworkLedDistrictHeatNetwork => "NetworkLedDistrictHeatNetwork",
                        ApiHeatNetworkType.DeveloperLedDistrictHeatNetworkMorL => "DeveloperLedDistrictHeatNetworkMorL",
                        ApiHeatNetworkType.DeveloperLedDistrictHeatNetworkSm => "DeveloperLedDistrictHeatNetworkSm",
                        ApiHeatNetworkType.CommunalHeatNetwork => "CommunalHeatNetwork",
                        _ => null
                    };
                }
                
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkType(HeatNetworkTypeViewModel model)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            model.HeatNetworkTypes = Helpers.Utility.GetHeatNetworkTypeOptions();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.SelectedHeatNetworkType)
            {
                case "NetworkLedDistrictHeatNetwork":
                    model.SelectedHeatNetworkTypeToDisplay = "Network‑led District Heat Network";
                    model.SelectedHeatNetworkTypeInEnum = ApiHeatNetworkType.NetworkLedDistrictHeatNetwork;
                    break;
                case "DeveloperLedDistrictHeatNetworkMorL":
                    model.SelectedHeatNetworkTypeToDisplay = "Developer‑led District Heat Network(medium‑large)";
                    model.SelectedHeatNetworkTypeInEnum = ApiHeatNetworkType.DeveloperLedDistrictHeatNetworkMorL;
                    break;
                case "DeveloperLedDistrictHeatNetworkSm":
                    model.SelectedHeatNetworkTypeToDisplay = "Developer‑led District Heat Network(small)";
                    model.SelectedHeatNetworkTypeInEnum = ApiHeatNetworkType.DeveloperLedDistrictHeatNetworkSm;
                    break;
                case "CommunalHeatNetwork":
                    model.SelectedHeatNetworkTypeToDisplay = "Communal Heat Network";
                    model.SelectedHeatNetworkTypeInEnum = ApiHeatNetworkType.CommunalHeatNetwork;
                    break;
                default:
                    model.SelectedHeatNetworkTypeToDisplay = null;
                    model.SelectedHeatNetworkTypeInEnum = null;
                    break;
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey, model);
            switch (model.SelectedHeatNetworkType)
            {
                case "NetworkLedDistrictHeatNetwork":
                    return RedirectToAction("NetworkSupply");
                case "DeveloperLedDistrictHeatNetworkMorL":
                case "DeveloperLedDistrictHeatNetworkSm":
                    return RedirectToAction("DoesDistrictNetworkContainPressureBreak");
                case "CommunalHeatNetwork":
                    return RedirectToAction("WhatIsTheHeatGenerationSourceFor");
                default:
                    return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> WhatIsTheHeatGenerationSourceForAsync()
        {
            this.ShowBackButton("HeatNetworkType");
            var model = _sessionHelper.GetFromSession<WhatIsTheHeatGenerationSourceForViewModel>(HttpContext, SessionKeys.WhatIsTheHeatGenerationSourceForViewModelSessionKey);
            if (model == null)
            {
                model = new WhatIsTheHeatGenerationSourceForViewModel();
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                var heatNetworkCharacteristics = heatNetworkData?.NetworkCharacteristics;

                if (heatNetworkCharacteristics != null)
                {
                    model.SelectedHeatGenerationSourceFor = heatNetworkCharacteristics.HeatGenerationSourceFor!;                    
                }                
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WhatIsTheHeatGenerationSourceFor(WhatIsTheHeatGenerationSourceForViewModel model)
        {
            this.ShowBackButton("HeatNetworkType");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.SelectedHeatGenerationSourceFor)
            {
                case "connectedToNetworkLedDistrictHN":
                    model.SelectedHeatGenerationSourceForToDisplay = "Connection to a network-led District Heat Network";
                    break;
                case "connectedToDeveloperLedDistrictHN":
                    model.SelectedHeatGenerationSourceForToDisplay = "Connection to a developer-led District Heat Network";
                    break;
                case "heatGeneratedInBuilding":
                    model.SelectedHeatGenerationSourceForToDisplay = "Heat generated in building";
                    break;
                default:
                    model.SelectedHeatGenerationSourceForToDisplay = null;
                    break;
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.WhatIsTheHeatGenerationSourceForViewModelSessionKey, model);
            return RedirectToAction("HowManyFloorsDoesTheCommunalHNServe");
        }

        [HttpGet]
        public async Task<IActionResult> HowManyFloorsDoesTheCommunalHNServeAsync()
        {
            this.ShowBackButton("WhatIsTheHeatGenerationSourceFor");
            var model = _sessionHelper.GetFromSession<CommunalFloorsViewModel>(HttpContext, SessionKeys.CommunalFloorsViewModelSessionKey);
            if (model == null)
            {
                model = new CommunalFloorsViewModel();
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                var heatNetworkCharacteristics = heatNetworkData?.NetworkCharacteristics;

                if (heatNetworkCharacteristics != null)
                {
                    model.NumberOfCommunalFloors = heatNetworkCharacteristics.NumberOfCommunalFloors ?? 0;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HowManyFloorsDoesTheCommunalHNServe(CommunalFloorsViewModel model)
        {
            this.ShowBackButton("WhatIsTheHeatGenerationSourceFor");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.CommunalFloorsViewModelSessionKey, model);
            return RedirectToAction("DoesDistrictNetworkContainPressureBreak");
        }

        [HttpGet]
        public async Task<IActionResult> DoesDistrictNetworkContainPressureBreakAsync()
        {
            var hnType = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey)?.SelectedHeatNetworkType;
            if (hnType == "DeveloperLedDistrictHeatNetworkMorL" || hnType == "DeveloperLedDistrictHeatNetworkSm")
            {
                this.ShowBackButton("HeatNetworkType");
            }
            else if (hnType == "CommunalHeatNetwork")
            {
                this.ShowBackButton("HowManyFloorsDoesTheCommunalHNServe");
            }
            var model = _sessionHelper.GetFromSession<DoesDistrictNetworkContainPressureBreakViewModel>(HttpContext, SessionKeys.DoesDistrictNetworkContainPressureBreakViewModelSessionKey);

            if (model == null)
            {
                model = new DoesDistrictNetworkContainPressureBreakViewModel();
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                var heatNetworkCharacteristics = heatNetworkData?.NetworkCharacteristics;

                // Safely handle nullable bool properties
                model.ContainsPressureBreak = heatNetworkCharacteristics?.ContainsPressureBreak == true ? "yes" : "no";
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoesDistrictNetworkContainPressureBreak(DoesDistrictNetworkContainPressureBreakViewModel model)
        {
            var hnType = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey)?.SelectedHeatNetworkType;
            if (hnType == "DeveloperLedDistrictHeatNetworkMorL" || hnType == "DeveloperLedDistrictHeatNetworkSm")
            {
                this.ShowBackButton("HeatNetworkType");
            }
            else if (hnType == "CommunalHeatNetwork")
            {
                this.ShowBackButton("HowManyFloorsDoesTheCommunalHNServe");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.DoesDistrictNetworkContainPressureBreakViewModelSessionKey, model);
            return RedirectToAction("NetworkSupply");
        }

        [HttpGet]
        public async Task<IActionResult> NetworkSupplyAsync()
        {
            var hnType = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey)?.SelectedHeatNetworkType;
            if (hnType == "NetworkLedDistrictHeatNetwork")
            {
                this.ShowBackButton("HeatNetworkType");
            }
            else
            {
                this.ShowBackButton("DoesDistrictNetworkContainPressureBreak");
            }
            var model = _sessionHelper.GetFromSession<NetworkSupplyViewModel>(HttpContext, SessionKeys.NetworkSupplyViewModelSessionKey);

            if (model == null)
            {
                model = new NetworkSupplyViewModel();
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                var heatNetworkCharacteristics = heatNetworkData?.NetworkCharacteristics;

                // Safely handle nullable bool properties
                model.IsSupplyingOtherHeatNetworks = heatNetworkCharacteristics?.IsSupplyingOtherHeatNetworks ?? false;
                model.HasCommercialConnections = heatNetworkCharacteristics?.HasCommercialConnections ?? false;
                model.IsSuppliedByADistrictHeatNetwork = heatNetworkCharacteristics?.IsSuppliedByADistrictHeatNetwork ?? false;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NetworkSupply(NetworkSupplyViewModel model)
        {
            var hnType = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey)?.SelectedHeatNetworkType;
            if (hnType == "NetworkLedDistrictHeatNetwork")
            {
                this.ShowBackButton("HeatNetworkType");
            }
            else
            {
                this.ShowBackButton("DoesDistrictNetworkContainPressureBreak");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkSupplyViewModelSessionKey, model);
            return RedirectToAction("NetworkOverView");
        }

        [HttpGet]
        public IActionResult NetworkOverView()
        {
            this.ShowBackButton("NetworkSupply");
            var heatNetworkType = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey);            
            var whatIsTheHeatGenerationSourceFor = _sessionHelper.GetFromSession<WhatIsTheHeatGenerationSourceForViewModel>(HttpContext, SessionKeys.WhatIsTheHeatGenerationSourceForViewModelSessionKey) ?? null;            
            var howManyFloorsDoesTheCommunalNetworkServe = _sessionHelper.GetFromSession<CommunalFloorsViewModel>(HttpContext, SessionKeys.CommunalFloorsViewModelSessionKey) ?? null;
            var doesTheDistrictNetworkContainAPressureBreak = _sessionHelper.GetFromSession<DoesDistrictNetworkContainPressureBreakViewModel>(HttpContext, SessionKeys.DoesDistrictNetworkContainPressureBreakViewModelSessionKey) ?? null;
            var networkSupply = _sessionHelper.GetFromSession<NetworkSupplyViewModel>(HttpContext, SessionKeys.NetworkSupplyViewModelSessionKey);
            var networkSupplyList = new List<string>();
            if (networkSupply.IsSupplyingOtherHeatNetworks) { networkSupplyList.Add("Child connections(Are you supplying any other networks)"); }
            if (networkSupply.HasCommercialConnections) { networkSupplyList.Add("Commercial connections(hotel, office)"); }
            if (networkSupply.IsSuppliedByADistrictHeatNetwork) { networkSupplyList.Add("Parent connection(Are you being supplied by another network)"); }

            var viewModel = new NetworkOverviewViewModel
            {
                SelectedHeatNetworkType = heatNetworkType.SelectedHeatNetworkTypeToDisplay,
                SelectedHeatGenerationSourceFor = whatIsTheHeatGenerationSourceFor?.SelectedHeatGenerationSourceFor,
                NumberOfCommunalFloors = howManyFloorsDoesTheCommunalNetworkServe?.NumberOfCommunalFloors,
                ContainsPressureBreak = doesTheDistrictNetworkContainAPressureBreak?.ContainsPressureBreak,
                NetworkSupply = networkSupplyList
            };            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NetworkOverView(NetworkOverviewViewModel model)
        {
            this.ShowBackButton("NetworkSupply");
            var heatNetworkType = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelSessionKey);
            var whatIsTheHeatGenerationSourceFor = _sessionHelper.GetFromSession<WhatIsTheHeatGenerationSourceForViewModel>(HttpContext, SessionKeys.WhatIsTheHeatGenerationSourceForViewModelSessionKey) ?? null;
            var howManyFloorsDoesTheCommunalNetworkServe = _sessionHelper.GetFromSession<CommunalFloorsViewModel>(HttpContext, SessionKeys.CommunalFloorsViewModelSessionKey) ?? null;
            var doesTheDistrictNetworkContainAPressureBreak = _sessionHelper.GetFromSession<DoesDistrictNetworkContainPressureBreakViewModel>(HttpContext, SessionKeys.DoesDistrictNetworkContainPressureBreakViewModelSessionKey) ?? null;
            var networkSupply = _sessionHelper.GetFromSession<NetworkSupplyViewModel>(HttpContext, SessionKeys.NetworkSupplyViewModelSessionKey);            
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var dataModel = new HNTAS.Api.Client.Model.NetworkCharacteristics2
            {
                HeatNetworkType = heatNetworkType.SelectedHeatNetworkTypeInEnum,
                HeatGenerationSourceFor = whatIsTheHeatGenerationSourceFor?.SelectedHeatGenerationSourceFor,
                NumberOfCommunalFloors = howManyFloorsDoesTheCommunalNetworkServe?.NumberOfCommunalFloors,
                ContainsPressureBreak = doesTheDistrictNetworkContainAPressureBreak?.ContainsPressureBreak == "yes" ? true : false,
                IsSupplyingOtherHeatNetworks = networkSupply.IsSupplyingOtherHeatNetworks,
                HasCommercialConnections = networkSupply.HasCommercialConnections,
                IsSuppliedByADistrictHeatNetwork = networkSupply.IsSuppliedByADistrictHeatNetwork,
                Status = HNTAS.Api.Client.Model.NetworkDetailsStatus.Complete,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _heatNetworkService.UpdateNetworkCharacteristics(hnId, dataModel);
            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }
    }
}