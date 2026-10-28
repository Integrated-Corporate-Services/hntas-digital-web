using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddNetworkElements)]
    public class NetworkElementsController : Controller
    {
        private readonly ILogger<NetworkElementsController> _logger;
        private readonly IHeatNetworkService _heatNetworkService;        
        private readonly ISessionHelper _sessionHelper;              

        public NetworkElementsController(ILogger<NetworkElementsController> logger, IHeatNetworkService heatNetworkService,  ISessionHelper sessionHelper)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;            
            _sessionHelper = sessionHelper;                    
        }        

        [HttpGet]
        public async Task<IActionResult> NetworkElementsListAsync()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkEcNameViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.EcSupplyCheckViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.EcSupplyViewModelSessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            var model = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);

            if (model != null)
            {
                ViewBag.NetworkBuildStatus = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.NetworkBuildStatusSessionKey); ;
                return View(model);
            }

            model = new NetworkElementViewModel();
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var networkType = heatNetworkData?.HeatNetworkType;
            var networkBuildStatus = heatNetworkData?.NetworkBuildStatus;

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkBuildStatusSessionKey, networkBuildStatus?.ToString());
            ViewBag.NetworkBuildStatus = networkBuildStatus?.ToString();
            model.ElementOptions = NetworkElementHelper.GetNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)networkType);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NetworkElementsListAsync(NetworkElementViewModel model)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            var networkBuildStatus = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.NetworkBuildStatusSessionKey);
            ViewBag.NetworkBuildStatus = networkBuildStatus;
            foreach (var option in model.ElementOptions)
            {                
                if (option.ExistingCount.HasValue && (option.ExistingCount.Value < 0 || option.ExistingCount.Value > 9999))
                {
                    ModelState.AddModelError($"ElementOptions[{model.ElementOptions.IndexOf(option)}].ExistingCount", $"Number of existing {option.Label?.ToLower()} must be 0 or more");
                }

                if (networkBuildStatus == (NetworkBuildStatus.NewAndExisting.ToString()) && !option.ExistingCount.HasValue)
                {
                    ModelState.AddModelError($"ElementOptions[{model.ElementOptions.IndexOf(option)}].ExistingCount", $"Enter number of existing {option.Label?.ToLower()}");
                }

                if (option.NewCount.HasValue && (option.NewCount.Value < 0 || option.NewCount.Value > 9999))
                {
                    ModelState.AddModelError($"ElementOptions[{model.ElementOptions.IndexOf(option)}].NewCount", $"Number of new {option.Label?.ToLower()} must be 0 or more");
                }
                if (option.Id == HeatNetworkElementType.EnergyCentre && option.NewCount.HasValue && option.NewCount.Value == 0)
                {
                    ModelState.AddModelError($"ElementOptions[{model.ElementOptions.IndexOf(option)}].NewCount", $"You mentioned previously that you have a main energy centre. You need to provide atleast one energy centre");
                }
                if (!option.NewCount.HasValue)
                {
                    ModelState.AddModelError($"ElementOptions[{model.ElementOptions.IndexOf(option)}].NewCount", $"Enter number of new {option.Label?.ToLower()}");
                }
            }            
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey, model);
            return RedirectToAction("NetworkEnergyCentreName");
        }

        [HttpGet]
        public async Task<IActionResult> NetworkEnergyCentreName()
        {
            this.ShowBackButton("NetworkElementsList", "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            var networkBuildStatus = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.NetworkBuildStatusSessionKey);
            ViewBag.NetworkBuildStatus = networkBuildStatus;

            var model = _sessionHelper.GetFromSession<NetworkEcNameViewModel>(HttpContext, SessionKeys.NetworkEcNameViewModelSessionKey);
            if (model != null)
            {
                return View(model);
            }
            var existingNetworkElement = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);
            var ecOption = existingNetworkElement?.ElementOptions.FirstOrDefault(x => x.Id == HeatNetworkElementType.EnergyCentre);

            model = NetworkElementHelper.GetNetworkEcName(ecOption!);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NetworkEnergyCentreName(NetworkEcNameViewModel model)
        {
            this.ShowBackButton("NetworkElementsList", "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            var networkBuildStatus = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.NetworkBuildStatusSessionKey);
            ViewBag.NetworkBuildStatus = networkBuildStatus;
            var nextAction = "NetworkElementSummary";

            foreach (var existingEc in model.ExistingEcNameOptions)
            {
                nextAction = "EnergyCentreSupplyCheck";
                if (string.IsNullOrWhiteSpace(existingEc.EcName))
                {
                    var index = model.ExistingEcNameOptions.IndexOf(existingEc);
                    ModelState.AddModelError($"existingecnameoptions[{index}].ecname", $"Enter the name for Existing energy centre {index + 1}");
                }
            }
            foreach (var newEc in model.NewEcNameOptions)
            {
                if (string.IsNullOrWhiteSpace(newEc.EcName))
                {
                    var index = model.NewEcNameOptions.IndexOf(newEc);
                    if (newEc.IsMainEc)
                    {
                        ModelState.AddModelError($"newecnameoptions[{index}].ecname", $"Enter the name of the main energy centre {index + 1}");
                    }
                    else
                    {
                        ModelState.AddModelError($"newecnameoptions[{index}].ecname", $"Enter the name of the new energy centre {index + 1}");
                    }
                }
            }            

            if (networkBuildStatus == NetworkBuildStatus.NewAndExisting.ToString())
            {                
                var allEcNames = model.ExistingEcNameOptions.Select(x => x.EcName?.Trim().ToLower()).Concat(model.NewEcNameOptions.Select(x => x.EcName?.Trim().ToLower())).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                var duplicateEcNames = allEcNames.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateEcNames.Any())
                {
                    foreach (var duplicateEcName in duplicateEcNames)
                    {
                        var existingEc = model.ExistingEcNameOptions.Where(x => x.EcName?.Trim().ToLower() == duplicateEcName);
                        if (existingEc != null)
                        {
                            foreach (var ec in existingEc)
                            {
                                var index = model.ExistingEcNameOptions.IndexOf(ec);
                                ModelState.AddModelError($"existingecnameoptions[{index}].ecname", $"Enter a different name for each energy centre (energy centre {index + 1})");
                            }
                        }
                        var newEc = model.NewEcNameOptions.Where(x => x.EcName?.Trim().ToLower() == duplicateEcName);
                        if (newEc != null)
                        {
                            foreach (var ec in newEc)
                            {
                                var index = model.NewEcNameOptions.IndexOf(ec);
                                if (ec.IsMainEc)
                                {
                                    ModelState.AddModelError($"newecnameoptions[{index}].ecname", $"Enter a different name for each energy centre (main energy centre {index + 1})");
                                }
                                else
                                {
                                    ModelState.AddModelError($"newecnameoptions[{index}].ecname", $"Enter a different name for each energy centre (new energy centre {index + 1})");
                                }
                            }
                        }
                    }
                }
            }

            if (networkBuildStatus == NetworkBuildStatus.New.ToString())
            {                
                var allNewEcNames = model.NewEcNameOptions.Select(x => x.EcName?.Trim().ToLower()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                var duplicateNewEcNames = allNewEcNames.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateNewEcNames.Any())
                {
                    foreach (var duplicateEcName in duplicateNewEcNames)
                    {
                        var newEc = model.NewEcNameOptions.Where(x => x.EcName?.Trim().ToLower() == duplicateEcName);
                        if (newEc != null)
                        {
                            foreach (var ec in newEc)
                            {
                                var index = model.NewEcNameOptions.IndexOf(ec);
                                if (ec.IsMainEc)
                                {
                                    ModelState.AddModelError($"newecnameoptions[{index}].ecname", $"Enter a different name for each energy centre (main energy centre {index + 1})");
                                }
                                else
                                {
                                    ModelState.AddModelError($"newecnameoptions[{index}].ecname", $"Enter a different name for each energy centre (new energy centre {index + 1})");
                                }
                            }
                        }
                    }
                }
            }            


            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkEcNameViewModelSessionKey, model);
            
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult EnergyCentreSupplyCheck()
        {
            this.ShowBackButton("NetworkEnergyCentreName", "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var model = _sessionHelper.GetFromSession<EnergyCentreSupplyViewModel>(HttpContext, SessionKeys.EcSupplyCheckViewModelSessionKey) ?? new EnergyCentreSupplyViewModel();
            if (model != null)
            {
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnergyCentreSupplyCheck(EnergyCentreSupplyViewModel model)
        {
            this.ShowBackButton("NetworkEnergyCentreName", "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.EcSupplyCheckViewModelSessionKey, model);
            
            if (model.IsExistingEcSupply == "yes")
            {
                return RedirectToAction("EnergyCentresSupply", "NetworkElements");
            }                 
            else
            {                
                _sessionHelper.ClearFromSession(HttpContext, SessionKeys.EcSupplyViewModelSessionKey);
            }
                
            return RedirectToAction("NetworkElementSummary");
        }

        [HttpGet]
        public IActionResult EnergyCentresSupply()
        {
            this.ShowBackButton("EnergyCentreSupplyCheck", "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var model = _sessionHelper.GetFromSession<EnergyCentresSupplyViewModel>(HttpContext, SessionKeys.EcSupplyViewModelSessionKey);

            var existingNetworkEcsModel = _sessionHelper.GetFromSession<NetworkEcNameViewModel>(HttpContext, SessionKeys.NetworkEcNameViewModelSessionKey);
            if (model != null)
            {
                return View(model);
            }
            model = new EnergyCentresSupplyViewModel();
            var existingNetworkEcs = existingNetworkEcsModel?.ExistingEcNameOptions.Select(x => x.EcName).ToList();
            model.ExistingEnergyCentresSupply.AddRange(existingNetworkEcs!.Select(x => new ExistingEnergyCentreSupply { EcName = x }));
                        
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnergyCentresSupply(EnergyCentresSupplyViewModel model)
        {
            this.ShowBackButton("EnergyCentreSupplyCheck", "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            var selectedEcNames = model.ExistingEnergyCentresSupply.Where(x => x.IsChecked == true).Select(x => x.EcName).ToList();
            if (!selectedEcNames.Any())
            {
                ModelState.AddModelError("existingenergycentressupply[0].ischecked", "Select at least one existing energy centre");
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.EcSupplyViewModelSessionKey, model);
            return RedirectToAction("NetworkElementSummary");
        }

        [HttpGet]
        public IActionResult NetworkElementSummary()
        {
            var supplyToNewBuilding = _sessionHelper.GetFromSession<EnergyCentreSupplyViewModel>(HttpContext, SessionKeys.EcSupplyCheckViewModelSessionKey);
            var networkElementsList = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);
            var networkEcNames = _sessionHelper.GetFromSession<NetworkEcNameViewModel>(HttpContext, SessionKeys.NetworkEcNameViewModelSessionKey);
            var existingEcSupplyingNewBuilding = _sessionHelper.GetFromSession<EnergyCentresSupplyViewModel>(HttpContext, SessionKeys.EcSupplyViewModelSessionKey);
            var networkBuildStatus = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.NetworkBuildStatusSessionKey);

            var backAction = SummaryBackAction(networkBuildStatus, networkElementsList, supplyToNewBuilding);
            
            this.ShowBackButton(backAction, "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            
            var model = new NetworkElementSummaryViewModel();
            foreach (var option in networkElementsList!.ElementOptions)
            {
                if (option.ExistingCount.HasValue && option.ExistingCount.Value > 0)
                {
                    model.ExistingNetworkElements ??= new List<string>();
                    model.ExistingNetworkElements.Add($"{option.ExistingCount.Value} existing {option.Label?.ToLower()}");
                }
                if (option.NewCount.HasValue && option.NewCount.Value > 0)
                {
                    model.NewNetworkElements ??= new List<string>();
                    if (option.Id == HeatNetworkElementType.EnergyCentre)
                        model.NewNetworkElements.Add($"{option.NewCount.Value} main {option.Label?.ToLower()}");
                    else
                        model.NewNetworkElements.Add($"{option.NewCount.Value} new {option.Label?.ToLower()}");
                }
            }

            foreach (var ec in networkEcNames!.ExistingEcNameOptions)
            {
                model.ExistingNetworkEcNames ??= new List<string>();
                model.ExistingNetworkEcNames.Add($"Existing {ec.EcName!}");
            }

            foreach (var ec in networkEcNames!.NewEcNameOptions)
            {
                model.NewNetworkEcNames ??= new List<string>();
                if (ec.IsMainEc)
                    model.NewNetworkEcNames.Add($"Main {ec.EcName!}");
                else
                    model.NewNetworkEcNames.Add($"New {ec.EcName!}");
            }
            
            ViewBag.SupplyToNewBuildings = supplyToNewBuilding != null && supplyToNewBuilding!.IsExistingEcSupply == "yes";
            if (supplyToNewBuilding != null && supplyToNewBuilding!.IsExistingEcSupply == "yes")
            {                
                var selectedEcNames = existingEcSupplyingNewBuilding?.ExistingEnergyCentresSupply.Where(x => x.IsChecked == true).Select(x => x.EcName).ToList();
                foreach (var ecName in selectedEcNames)
                {
                    model.ExistingEcSupplyingNewBuilding ??= new List<string>();
                    model.ExistingEcSupplyingNewBuilding.Add($"Existing {ecName}");
                }
            }            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NetworkElementSummaryAsync(NetworkElementSummaryViewModel model)
        {
            var supplyToNewBuilding = _sessionHelper.GetFromSession<EnergyCentreSupplyViewModel>(HttpContext, SessionKeys.EcSupplyCheckViewModelSessionKey);
            var networkElementsList = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);
            var networkEcNames = _sessionHelper.GetFromSession<NetworkEcNameViewModel>(HttpContext, SessionKeys.NetworkEcNameViewModelSessionKey);
            var existingEcSupplyingNewBuilding = _sessionHelper.GetFromSession<EnergyCentresSupplyViewModel>(HttpContext, SessionKeys.EcSupplyViewModelSessionKey);
            var networkBuildStatus = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.NetworkBuildStatusSessionKey);
            
            var backAction = SummaryBackAction(networkBuildStatus, networkElementsList, supplyToNewBuilding);
            
            this.ShowBackButton(backAction, "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);            
                        
            if (model.ConfirmedDeclaration != true)
            {                
                return View(model);
            }

            var elementGroupList = new List<ElementGroup>();
            var elementList = new List<Element>();
            var instanceCounter = 1;
            foreach (var option in networkElementsList!.ElementOptions)
            {
                var elementType = NetworkElementHelper.GetNetworkElementIdByType(option.Id.ToString()!);
                if (option.ExistingCount.HasValue && option.ExistingCount.Value > 0)
                {
                    //var elementGroup = new ElementGroup
                    //{
                    //    BuildType = NetworkBuildStatus.Existing,
                    //    Count = option.ExistingCount.Value,
                    //    ElementDisplayType = option.Id,
                    //    ElementType = elementType
                    //};
                    //elementGroupList.Add(elementGroup);

                    for (int cnt = 0; cnt < option.ExistingCount.Value; cnt++)
                    {
                        var el = new Element
                        {
                            BuildType = NetworkBuildStatus.Existing,
                            ElementType = elementType,
                            ElementId = (instanceCounter).ToString("D5"),
                            NetworkElementInstanceName = NetworkElementHelper.GetNetworkElementLabelByElementType(option.Id) + " - " + instanceCounter,
                            
                        };
                        elementList.Add(el);
                        instanceCounter++;
                    }
                }
                if (option.NewCount.HasValue && option.NewCount.Value > 0)
                {                    
                    var elementGroup = new ElementGroup
                    {
                        BuildType = NetworkBuildStatus.New,
                        Count = option.NewCount.Value,
                        ElementDisplayType = option.Id,
                        ElementType = elementType
                    };
                    elementGroupList.Add(elementGroup);
                    for (int cnt = 0; cnt < option.NewCount.Value; cnt++)
                    {
                        var el = new Element
                        {
                            BuildType = NetworkBuildStatus.New,
                            ElementType = elementType,
                            ElementId = (instanceCounter).ToString("D5"),
                            NetworkElementInstanceName = NetworkElementHelper.GetNetworkElementLabelByElementType(option.Id) + " - " + instanceCounter,
                            
                        };
                        elementList.Add(el);
                        instanceCounter++;
                    }
                }
            }

            var newElements = elementList.Where(x => x.BuildType == NetworkBuildStatus.New).ToList();
            var newElementsWithEcNames = networkEcNames!.NewEcNameOptions;
            var existingElements = elementList.Where(x => x.BuildType == NetworkBuildStatus.Existing).ToList();
            var existingElementsWithEcNames = networkEcNames!.ExistingEcNameOptions;            
            
            var selectedEcNames = (supplyToNewBuilding != null && supplyToNewBuilding.IsExistingEcSupply == "yes") ? existingEcSupplyingNewBuilding?.ExistingEnergyCentresSupply.Where(x => x.IsChecked == true).Select(x => x.EcName).ToList() : null;

            for (int i = 0; i< newElementsWithEcNames.Count; i++)
            {
                var newElement = newElements[i];
                var newEcName = newElementsWithEcNames[i];
                newElement.ElementName = newEcName.EcName;
                newElement.IsMainEnergyCentre = newEcName.IsMainEc;
                newElement.IsSupplyNewBuildings = true;
            }

            for (int i = 0; i < existingElementsWithEcNames.Count; i++)
            {
                var existingElement = existingElements[i];
                var existingEcName = existingElementsWithEcNames[i];
                existingElement.ElementName = existingEcName.EcName;
                existingElement.IsSupplyNewBuildings = selectedEcNames != null && selectedEcNames.Contains(existingEcName.EcName);
            }

            var request = new NetworkElements2
            {
                ElementsGroup = elementGroupList,
                Elements = elementList,
                IsExistingEnergyCentreSupplyNewBuildings = supplyToNewBuilding != null && supplyToNewBuilding!.IsExistingEcSupply == "yes",
                NetworkElementStatus = NullableOfNetworkDetailsStatus.Complete,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                ElementSoaStatus = NullableOfNetworkDetailsStatus.ReadyToStart,
                EcPlantDataStatus = NullableOfNetworkDetailsStatus.ReadyToStart
            };

            await _heatNetworkService.UpdateNetworkElements(hnId!, request);
            ClearNetworkElementSpecificSession();

            return RedirectToAction("NetworkConfirmation");
        }

        [HttpGet]
        public IActionResult NetworkConfirmationAsync()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> NetworkElementsOverViewAsync()
        {            
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var model = new NetworkElementsOverViewModel();
            // Network Connections
            var networkConnections = heatNetworkData?.HeatNetworkConnections;
            model.NetworkConnections.ExistingCommunalBuildings = networkConnections?.NoOfExistingCommunalBuilding;
            model.NetworkConnections.ExistingHousesOrMaisonettes = networkConnections?.NoOfExistingHousesOrMaisonettes;
            model.NetworkConnections.ExistingNonDomesticBuildings = networkConnections?.NoOfExistingNonDomesticBuildings;
            model.NetworkConnections.ExistingMultiTenantedNonDomestic = networkConnections?.NoOfExistingMultiTenantedNonDomestic;
            model.NetworkConnections.ExistingOtherDistrictNetworks = networkConnections?.NoOfExistingOtherDistrictNetworks;
            model.NetworkConnections.NewCommunalBuildings = networkConnections?.NoOfNewCommunalBuilding;
            model.NetworkConnections.NewHousesOrMaisonettes = networkConnections?.NoOfNewHousesOrMaisonettes;
            model.NetworkConnections.NewNonDomesticBuildings = networkConnections?.NoOfNewNonDomesticBuildings;
            model.NetworkConnections.NewMultiTenantedNonDomestic = networkConnections?.NoOfNewMultiTenantedNonDomestic;
            model.NetworkConnections.NewOtherDistrictNetworks = networkConnections?.NoOfNewOtherDistrictNetworks;
            model.EnergyCentreType = heatNetworkData?.EnergyCentreType?.ToString() ?? string.Empty;
            model.NetworkBuildStatus = heatNetworkData?.NetworkBuildStatus?.ToString() ?? string.Empty;
            var networkElements = heatNetworkData?.NetworkElements;
            
            var newElementGroups = networkElements?.ElementsGroup?.Where(x => x.BuildType == NetworkBuildStatus.New).ToList();
            var existingElementGroups  = networkElements?.ElementsGroup?.Where(x => x.BuildType == NetworkBuildStatus.Existing).ToList();

            foreach (var newElementGroup in newElementGroups!)
            {
                var elementOption = NetworkElementHelper.GetNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)heatNetworkData?.HeatNetworkType).FirstOrDefault(x => x.Id == newElementGroup.ElementDisplayType);
                var label = elementOption != null ? elementOption.Label : newElementGroup.ElementDisplayType.ToString();
                label = label.ToSentenceCase();
                model.NewNetworkElements ??= new List<string>();
                model.NewNetworkElements.Add($"{newElementGroup.Count} new {label?.ToLower()}");
            }

            foreach (var existingElementGroup in existingElementGroups!)
            {
                var elementOption = NetworkElementHelper.GetNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)heatNetworkData?.HeatNetworkType).FirstOrDefault(x => x.Id == existingElementGroup.ElementDisplayType);
                var label = elementOption != null ? elementOption.Label : existingElementGroup.ElementDisplayType.ToString();
                label = label.ToSentenceCase();
                model.ExistingNetworkElements ??= new List<string>();
                model.ExistingNetworkElements.Add($"{existingElementGroup.Count} existing {label?.ToLower()}");
            }

            
            var newEcElements = networkElements.Elements?.Where(x => x.BuildType == NetworkBuildStatus.New && x.ElementType == ElementTypeInShort.EC).ToList();    
            foreach (var newEcElement in newEcElements!)
            {
                model.NewNetworkEcNames ??= new List<string>();
                model.NewNetworkEcNames.Add($"{newEcElement.ElementName}");
            }

            var existingEcElements = networkElements.Elements?.Where(x => x.BuildType == NetworkBuildStatus.Existing && x.ElementType == ElementTypeInShort.EC).ToList();
            foreach (var existingEcElement in existingEcElements!)
            {
                model.ExistingNetworkEcNames ??= new List<string>();
                model.ExistingNetworkEcNames.Add($"{existingEcElement.ElementName}");
            }

            var isExistingEcSupplyNewBuildings = networkElements.IsExistingEnergyCentreSupplyNewBuildings;
            ViewBag.SupplyToNewBuildings = isExistingEcSupplyNewBuildings;
            if (isExistingEcSupplyNewBuildings == true)
            {
                var existingEcSupplyingNewBuildings = networkElements.Elements?.Where(x => x.BuildType == NetworkBuildStatus.Existing && x.ElementType == ElementTypeInShort.EC && x.IsSupplyNewBuildings == true).ToList();
                foreach (var existingEcSupplyingNewBuilding in existingEcSupplyingNewBuildings!)
                {
                    model.ExistingEcSupplyingNewBuilding ??= new List<string>();
                    model.ExistingEcSupplyingNewBuilding.Add($"{existingEcSupplyingNewBuilding.ElementName}");
                }
            }            

            var address = heatNetworkData?.Address;
            var coordinates = heatNetworkData?.EcDetails;
            var phase = heatNetworkData?.Phase;
            var latlong = coordinates != null ? $"{coordinates.Latitude},{coordinates.Longitude}" : null;
            var addressByStreetOrTownModel = address != null ? (AddressByStreetOrTownModel)address! : null;

            model.Address = addressByStreetOrTownModel?.Fulladdress ?? "Not provided";
            model.Coordinates = latlong;
            model.Phase = phase ?? string.Empty;
            model.NetworkType = heatNetworkData!.HeatNetworkType.ToString();
            
            return View(model);
        }

        private string SummaryBackAction(string networkBuildStatus, NetworkElementViewModel networkElementsList, EnergyCentreSupplyViewModel supplyToNewBuilding)
        {
            var backAction = string.Empty;
            if (networkBuildStatus == NetworkBuildStatus.New.ToString())
            {
                backAction = "NetworkEnergyCentreName";
            }
            else
            {
                var ecInExistingNetworkElement = networkElementsList?.ElementOptions.FirstOrDefault(x => x.Id == HeatNetworkElementType.EnergyCentre)?.ExistingCount;
                if (ecInExistingNetworkElement.HasValue && ecInExistingNetworkElement.Value > 0)
                {
                    var ecSupplyToNewBuildCheck = supplyToNewBuilding ?? new EnergyCentreSupplyViewModel();
                    if (ecSupplyToNewBuildCheck.IsExistingEcSupply == "yes")
                    {
                        backAction = "EnergyCentresSupply";
                    }
                    else
                    {
                        backAction = "EnergyCentreSupplyCheck";
                    }
                }
                else
                {
                    backAction = "NetworkEnergyCentreName";
                }
            }
            return backAction;
        }

        private void ClearNetworkElementSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkEcNameViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.EcSupplyCheckViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.EcSupplyViewModelSessionKey);            
        }        
    }
}
