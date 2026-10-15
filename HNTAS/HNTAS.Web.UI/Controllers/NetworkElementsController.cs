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
using System.Xml.Linq;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetworkDetail)]
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

        //[HttpGet]
        //public async Task<IActionResult> SelectNetworkElementsAsync()
        //{
        //    this.ShowBackButton("NetworkDetails", "HeatNetwork");
        //    var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
        //    var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

        //    ViewBag.HnId = hnId?.ToUpper();
        //    ViewBag.HnName = hnName;
        //    var model = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);

        //    if (model != null)
        //    {
        //        return View(model);
        //    }

        //    model = new NetworkElementViewModel();
        //    var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
        //    var networkType = heatNetworkData?.HeatNetworkType;
        //    bool hasOwnEc = heatNetworkData?.HasOwnEnergyCentre ?? false;

        //    model.ElementOptions = NetworkElementHelper.GetNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc);
        //    ViewBag.Heading = NetworkElementHelper.GetNetworkElementHeadingForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc);
        //    var selectedNetworkElements = heatNetworkData?.NetworkElements;

        //    if (selectedNetworkElements != null)
        //    {
        //        selectedNetworkElements.ElementsGroup?.DistinctBy(a => a.ElementType).ToList().ForEach(e =>
        //        {
        //            var displayType = NetworkElementHelper.GetNetworkElementDisplayTypeById(e.ElementType!);
        //            model.SelectedElementIds.Add(displayType);
        //            if (e.Count.HasValue)
        //            {
        //                model.ElementCounts[displayType] = e.Count.Value;
        //            }
        //        });
        //    }

        //    return View(model);
        //}

        [HttpGet]
        public async Task<IActionResult> NewAndExistingNetworkElementsAsync()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            var model = _sessionHelper.GetFromSession<NewAndExistingNetworkElementViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkElementsViewModelSessionKey);

            if (model != null)
            {
                return View(model);
            }

            model = new NewAndExistingNetworkElementViewModel();
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var networkType = heatNetworkData?.HeatNetworkType;
            bool hasOwnEc = heatNetworkData?.HasOwnEnergyCentre ?? false;

            model.ElementOptions = NetworkElementHelper.GetNewAndExistingNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)networkType);
            //ViewBag.Heading = NetworkElementHelper.GetNetworkElementHeadingForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc);
            var selectedNetworkElements = heatNetworkData?.NetworkElements;            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewAndExistingNetworkElementsAsync(NewAndExistingNetworkElementViewModel model)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var networkType = heatNetworkData?.HeatNetworkType;
            bool hasOwnEc = heatNetworkData?.HasOwnEnergyCentre ?? false;
            
            foreach(var option in model.ElementOptions)
            {                
                if (!option.ExistingCount.HasValue)
                {
                    ModelState.AddModelError($"ElementOptions[{model.ElementOptions.IndexOf(option)}].ExistingCount", $"Enter number of existing {option.Label?.ToLower()}");
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
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewAndExistingNetworkElementsViewModelSessionKey, model);
            return RedirectToAction("NewAndExistingNetworkEnergyCentreName");
        }

        [HttpGet]
        public async Task<IActionResult> NewAndExistingNetworkEnergyCentreName()
        {
            this.ShowBackButton("NewAndExistingNetworkElements", "NetworkElements");

            var model = _sessionHelper.GetFromSession<NewAndExistingNetworkEcNameViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkEcNameViewModelSessionKey);
            if (model != null)
            {
                return View(model);
            }
            var existingNetworkElement = _sessionHelper.GetFromSession<NewAndExistingNetworkElementViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkElementsViewModelSessionKey);
            var ecOption = existingNetworkElement?.ElementOptions.FirstOrDefault(x => x.Id == HeatNetworkElementType.EnergyCentre);

            model = NetworkElementHelper.GetExistingNetworkEcName(ecOption!);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewAndExistingNetworkEnergyCentreName(NewAndExistingNetworkEcNameViewModel model)
        {
            this.ShowBackButton("NewAndExistingNetworkElements", "NetworkElements");
            foreach(var existingEc in model.ExistingEcNameOptions)
            {
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

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewAndExistingNetworkEcNameViewModelSessionKey, model);
            return RedirectToAction("NewAndExistingEnergyCentreSupplyCheck");
        }

        [HttpGet]
        public IActionResult NewAndExistingEnergyCentreSupplyCheck()
        {
            this.ShowBackButton("NewAndExistingNetworkEnergyCentreName", "NetworkElements");
            var model = _sessionHelper.GetFromSession<NewAndExistingEnergyCentreSupplyViewModel>(HttpContext, SessionKeys.NewAndExistingEcSupplyCheckViewModelSessionKey) ?? new NewAndExistingEnergyCentreSupplyViewModel();
            if (model != null)
            {
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewAndExistingEnergyCentreSupplyCheck(NewAndExistingEnergyCentreSupplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewAndExistingEcSupplyCheckViewModelSessionKey, model);
            
            if (model.IsExistingEcSupply == "yes")
                return RedirectToAction("NewAndExistingEnergyCentresSupply", "NetworkElements");            
            return RedirectToAction("SelectNetworkElements");
        }

        [HttpGet]
        public IActionResult NewAndExistingEnergyCentresSupply()
        {
            this.ShowBackButton("NewAndExistingEnergyCentreSupplyCheck", "NetworkElements");
            var model = _sessionHelper.GetFromSession<NewAndExistingEnergyCentresSupplyViewModel>(HttpContext, SessionKeys.NewAndExistingEcSupplyViewModelSessionKey);

            var existingNetworkEcsModel = _sessionHelper.GetFromSession<NewAndExistingNetworkEcNameViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkEcNameViewModelSessionKey);
            if (model != null)
            {
                return View(model);
            }
            model = new NewAndExistingEnergyCentresSupplyViewModel();
            var existingNetworkEcs = existingNetworkEcsModel?.ExistingEcNameOptions.Select(x => x.EcName).ToList();
            model.ExistingEnergyCentresSupply.AddRange(existingNetworkEcs!.Select(x => new ExistingEnergyCentreSupply { EcName = x }));
                        
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewAndExistingEnergyCentresSupply(NewAndExistingEnergyCentresSupplyViewModel model)
        {
            var selectedEcNames = model.ExistingEnergyCentresSupply.Where(x => x.IsChecked == true).Select(x => x.EcName).ToList();
            if (!selectedEcNames.Any())
            {
                ModelState.AddModelError("existingenergycentressupply[0].ischecked", "Select at least one existing energy centre");
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewAndExistingEcSupplyViewModelSessionKey, model);
            return RedirectToAction("NewAndExistingNetworkElementSummary");
        }

        [HttpGet]
        public IActionResult NewAndExistingNetworkElementSummary()
        {
            var existingNetworkElements = _sessionHelper.GetFromSession<NewAndExistingNetworkElementViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkElementsViewModelSessionKey);
            var existingNetworkEcNames = _sessionHelper.GetFromSession<NewAndExistingNetworkEcNameViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkEcNameViewModelSessionKey);
            var existingEcSupplyingNewBuilding = _sessionHelper.GetFromSession<NewAndExistingEnergyCentresSupplyViewModel>(HttpContext, SessionKeys.NewAndExistingEcSupplyViewModelSessionKey);

            var model = new NewAndExistingNetworkElementSummaryViewModel();

            foreach (var option in existingNetworkElements!.ElementOptions)
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

            foreach (var ec in existingNetworkEcNames!.ExistingEcNameOptions)
            {
                model.ExistingNetworkEcNames ??= new List<string>();
                model.ExistingNetworkEcNames.Add($"Existing {ec.EcName!}");
            }

            foreach (var ec in existingNetworkEcNames!.NewEcNameOptions)
            {
                model.NewNetworkEcNames ??= new List<string>();
                if (ec.IsMainEc)
                    model.NewNetworkEcNames.Add($"Main {ec.EcName!}");
                else
                    model.NewNetworkEcNames.Add($"New {ec.EcName!}");
            }

            var selectedEcNames = existingEcSupplyingNewBuilding?.ExistingEnergyCentresSupply.Where(x => x.IsChecked == true).Select(x => x.EcName).ToList();
            foreach (var ecName in selectedEcNames)
            {
                model.ExistingEcSupplyingNewBuilding ??= new List<string>();
                model.ExistingEcSupplyingNewBuilding.Add($"Existing { ecName }");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewAndExistingNetworkElementSummaryAsync(NewAndExistingNetworkElementSummaryViewModel model)
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var newAndExistingNetworkElements = _sessionHelper.GetFromSession<NewAndExistingNetworkElementViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkElementsViewModelSessionKey);
            var newAndExistingNetworkEcNames = _sessionHelper.GetFromSession<NewAndExistingNetworkEcNameViewModel>(HttpContext, SessionKeys.NewAndExistingNetworkEcNameViewModelSessionKey);
            var newAndExistingEcSupplyingNewBuilding = _sessionHelper.GetFromSession<NewAndExistingEnergyCentresSupplyViewModel>(HttpContext, SessionKeys.NewAndExistingEcSupplyViewModelSessionKey);
            var newAndExistingEnergyCentreSupplyNewBuildingCheck = _sessionHelper.GetFromSession<NewAndExistingEnergyCentreSupplyViewModel>(HttpContext, SessionKeys.NewAndExistingEcSupplyCheckViewModelSessionKey);

            if (model.ConfirmedDeclaration != true)
            {
                ModelState.AddModelError(nameof(model.ConfirmedDeclaration), "Confirm that the summary is correct");
                return View(model);
            }

            var elementGroupList = new List<ElementGroup>();
            var elementList = new List<Element>();
            var instanceCounter = 1;
            foreach (var option in newAndExistingNetworkElements!.ElementOptions)
            {
                if (option.ExistingCount.HasValue && option.ExistingCount.Value > 0)
                {
                    var elementGroup = new ElementGroup
                    {
                        BuildType = NetworkBuildStatus.Existing,
                        Count = option.ExistingCount.Value,
                        ElementDisplayType = option.Id,
                        ElementType = NetworkElementHelper.GetNetworkElementIdByType(option.Id.ToString()!)
                    };
                    elementGroupList.Add(elementGroup);

                    for (int cnt = 0; cnt < option.ExistingCount.Value; cnt++)
                    {
                        var el = new Element
                        {
                            BuildType = NetworkBuildStatus.Existing,
                            ElementType = NetworkElementHelper.GetNetworkElementIdByType(option.Id.ToString()!),
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
                        ElementType = NetworkElementHelper.GetNetworkElementIdByType(option.Id.ToString()!)
                    };
                    elementGroupList.Add(elementGroup);
                    for (int cnt = 0; cnt < option.NewCount.Value; cnt++)
                    {
                        var el = new Element
                        {
                            BuildType = NetworkBuildStatus.New,
                            ElementType = NetworkElementHelper.GetNetworkElementIdByType(option.Id.ToString()!),
                            ElementId = (instanceCounter).ToString("D5"),
                            NetworkElementInstanceName = NetworkElementHelper.GetNetworkElementLabelByElementType(option.Id) + " - " + instanceCounter,
                        };
                        elementList.Add(el);
                        instanceCounter++;
                    }
                }
            }

            var newElements = elementList.Where(x => x.BuildType == NetworkBuildStatus.New).ToList();
            var newElementsWithEcNames = newAndExistingNetworkEcNames!.NewEcNameOptions;
            var existingElements = elementList.Where(x => x.BuildType == NetworkBuildStatus.Existing).ToList();
            var existingElementsWithEcNames = newAndExistingNetworkEcNames!.ExistingEcNameOptions;

            var selectedEcNames = newAndExistingEcSupplyingNewBuilding?.ExistingEnergyCentresSupply.Where(x => x.IsChecked == true).Select(x => x.EcName).ToList();

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
                IsExistingEnergyCentreSupplyNewBuildings = newAndExistingEnergyCentreSupplyNewBuildingCheck!.IsExistingEcSupply == "yes",
                NetworkElementStatus = NetworkDetailsStatus.Complete,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                ElementSoaStatus = NetworkDetailsStatus.ReadyToStart
            };

            await _heatNetworkService.UpdateNetworkElements(hnId!, request);

            return View(model);
        }

        [HttpGet]
        public IActionResult NewAndExistingNetworkOverview()
        {
            return View();
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SelectNetworkElements(NetworkElementViewModel model)
        //{
        //    this.ShowBackButton("NetworkDetails", "HeatNetwork");
        //    var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
        //    var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
        //    ViewBag.HnId = hnId?.ToUpper();
        //    ViewBag.HnName = hnName;

        //    var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
        //    var networkType = heatNetworkData?.HeatNetworkType;
        //    bool hasOwnEc = heatNetworkData?.HasOwnEnergyCentre ?? false;

        //    model.ElementOptions = NetworkElementHelper.GetNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc);
        //    ViewBag.Heading = NetworkElementHelper.GetNetworkElementHeadingForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc);
        //    var elements = new List<ElementGroup>();

        //    foreach (var selectedId in model.SelectedElementIds)
        //    {
        //        var ele = new ElementGroup
        //        {
        //            ElementDisplayType = selectedId,
        //            Count = model.ElementCounts.TryGetValue(selectedId, out var cnt) ? (int?)cnt : null,
        //        };
        //        elements.Add(ele);


        //        var element = NetworkElementHelper.GetNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc).FirstOrDefault(x => x.Id == selectedId);

        //        if (element == null)
        //        {
        //            return BadRequest();
        //        }

        //        if (model.ElementCounts.TryGetValue(selectedId, out var count) && count.HasValue)
        //        {
        //            // Check if the double has a fractional part
        //            bool isWholeNumber = count.Value % 1 == 0;

        //            if (!isWholeNumber)
        //            {
        //                ModelState.Remove($"ElementCounts.{selectedId}");
        //                ModelState.AddModelError($"ElementCounts[{selectedId}]", $"Enter the number of {element.SubLabel.ToLower()} as a whole number");
        //            }
        //        }

        //        if (count == null || count <= 0)
        //        {

        //            if(count == null)
        //            {
        //                // Remove the automatic ModelState entry first
        //                ModelState.Remove($"ElementCounts.{selectedId}");
        //                ModelState.AddModelError($"ElementCounts[{selectedId}]", $"Enter the number of {element.SubLabel.ToLower()}");
        //            }
        //            else
        //            {
        //                ModelState.Remove($"ElementCounts.{selectedId}");
        //                ModelState.AddModelError($"ElementCounts[{selectedId}]", $"Enter the number of {element.SubLabel.ToLower()} as a whole number greater than 0");
        //            }
        //        }
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View("SelectNetworkElements", model);
        //    }

        //    var address = heatNetworkData?.Address;
        //    var coordinates = heatNetworkData?.EcDetails;
        //    var phase = heatNetworkData?.Phase;
        //    var latlong = coordinates != null ? $"{coordinates.Latitude},{coordinates.Longitude}" : null;
        //    var addressByStreetOrTownModel = address != null ? (AddressByStreetOrTownModel)address! : null;

        //    var networkElementsOverViewModel = new NetworkElementsOverViewModel
        //    {
        //        Elements = elements.Select(e =>
        //        {
        //            var elementOption = NetworkElementHelper.GetNetworkElementOptionsForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc).FirstOrDefault(x => x.Id == e.ElementDisplayType);
        //            var label = elementOption != null ? elementOption.Label : e.ElementDisplayType.ToString();
        //            label = label.ToSentenceCase();

        //            //return e.Count.HasValue ? $"{e.Count.Value} {label}(s)" : label;
        //            return e.Count.HasValue ? $"{e.Count.Value} {GetElementLabel(label, e.Count.Value)}" : label;
        //        }).ToList(),
        //        HeatNetworkAddress = addressByStreetOrTownModel?.Fulladdress ?? "Not provided",
        //        Coordinates = latlong,
        //        NetworkType = NetworkElementHelper.GetNetworkTypeLabelForNetworkType((Api.Client.Model.HeatNetworkType?)networkType, hasOwnEc),
        //        Phase = phase ?? string.Empty
        //    };

        //    var networkElementGroups = GetNetworkElementGroup(elements);
        //    _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey, networkElementsOverViewModel);
        //    _sessionHelper.SaveToSession(HttpContext, SessionKeys.SelectedElementsSessionKey, networkElementGroups);
        //    _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey, model);
        //    if ((Api.Client.Model.HeatNetworkType?)networkType == Api.Client.Model.HeatNetworkType.District)
        //    {
        //        return RedirectToAction("Substations", "NetworkElements");
        //    }
        //    return RedirectToAction("NetworkElementsOverView", "NetworkElements");
        //}

        //[HttpGet]
        //public IActionResult Substations()
        //{
        //    this.ShowBackButton("SelectNetworkElements", "NetworkElements");
        //    var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
        //    var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
        //    ViewBag.HnId = hnId?.ToUpper();
        //    ViewBag.HnName = hnName;
        //    var model = _sessionHelper.GetFromSession<SubstationsViewModel>(HttpContext, SessionKeys.SubstationViewModelKey);
        //    if (model != null)
        //    {
        //        return View(model);
        //    }
        //    model = new SubstationsViewModel();
        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Substations(SubstationsViewModel model)
        //{
        //    this.ShowBackButton("SelectNetworkElements", "NetworkElements");
        //    var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
        //    var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
        //    ViewBag.HnId = hnId?.ToUpper();
        //    ViewBag.HnName = hnName;

        //    if (model.HasDistrictSubstation == false)
        //    {
        //        model.NumberOfSubstations = null;
        //        ModelState.Remove(nameof(model.NumberOfSubstations));
        //    }
        //    else if (model.HasDistrictSubstation == true && (model.NumberOfSubstations == null))
        //    {
        //        ModelState.AddModelError("NumberOfSubstations", "Enter the number of substations");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    _sessionHelper.SaveToSession(HttpContext, SessionKeys.SubstationViewModelKey, model);
        //    return RedirectToAction("DistributionNetworks");
        //}


        //[HttpGet]
        //public IActionResult DistributionNetworks()
        //{
        //    this.ShowBackButton("Substations", "NetworkElements");
        //    var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
        //    var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
        //    ViewBag.HnId = hnId?.ToUpper();
        //    ViewBag.HnName = hnName;
        //    var model = _sessionHelper.GetFromSession<DistributionNetworksViewModel>(HttpContext, SessionKeys.DistributionNetworksViewModelKey);
        //    if (model != null)
        //    {
        //        return View(model);
        //    }
        //    model = new DistributionNetworksViewModel();
        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult DistributionNetworks(DistributionNetworksViewModel model)
        //{
        //    this.ShowBackButton("Substations", "NetworkElements");
        //    var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
        //    var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
        //    ViewBag.HnId = hnId?.ToUpper();
        //    ViewBag.HnName = hnName;
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    _sessionHelper.SaveToSession(HttpContext, SessionKeys.DistributionNetworksViewModelKey, model);


        //    var elementsGroup = _sessionHelper.GetFromSession<List<NetworkElementGroup>>(HttpContext, SessionKeys.SelectedElementsSessionKey);
        //    var elements = GetElementGroup(elementsGroup!);
        //    var substation = _sessionHelper.GetFromSession<SubstationsViewModel>(HttpContext, SessionKeys.SubstationViewModelKey);
        //    if (substation?.NumberOfSubstations > 0)
        //    {
        //        // Delete any existing substation element to avoid duplication in case user goes back and changes the number of substations
        //        elements?.RemoveAll(e => e.ElementDisplayType == HeatNetworkElementType.Substation);
        //        var substationElement = new ElementGroup
        //        {
        //            Count = substation?.NumberOfSubstations,
        //            ElementDisplayType = HeatNetworkElementType.Substation
        //        };
        //        elements?.Add(substationElement);
        //    }
        //    if (model?.NumberOfDistributionNetworks > 0)
        //    {
        //        // Delete any existing distribution network element to avoid duplication in case user goes back and changes the number of distribution networks
        //        elements?.RemoveAll(e => e.ElementDisplayType == HeatNetworkElementType.DistrictDistribution);
        //        var distributionNetworkElement = new ElementGroup
        //        {
        //            Count = model?.NumberOfDistributionNetworks,
        //            ElementDisplayType = HeatNetworkElementType.DistrictDistribution
        //        };
        //        elements?.Add(distributionNetworkElement);
        //    }
        //    var networkElementOverview = _sessionHelper.GetFromSession<NetworkElementsOverViewModel>(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey);
        //    networkElementOverview!.Elements = elements!.Select(e =>
        //    {                
        //        var elementOption = NetworkElementHelper.GetNetworkElementOptionsForNetworkType().FirstOrDefault(x => x.Id == e.ElementDisplayType);
        //        var label = elementOption != null ? elementOption.Label : e.ElementDisplayType.ToString();
        //        label = label.ToSentenceCase();

        //        //return e.Count.HasValue ? $"{e.Count.Value} {label}(s)" : label;
        //        return e.Count.HasValue? $"{e.Count.Value} {GetElementLabel(label, e.Count.Value)}" : label;
        //    }).ToList();

        //    _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey, networkElementOverview);
        //    _sessionHelper.SaveToSession(HttpContext, SessionKeys.SelectedElementsSessionKey, elements);

        //    return RedirectToAction("NetworkElementsOverView");
        //}

        //[HttpGet]
        //public IActionResult NetworkElementsOverView()
        //{
        //    var substationModel = _sessionHelper.GetFromSession<SubstationsViewModel>(HttpContext, SessionKeys.SubstationViewModelKey);
        //    if (substationModel != null)
        //        this.ShowBackButton("DistributionNetworks", "NetworkElements");
        //    else
        //        this.ShowBackButton("SelectNetworkElements", "NetworkElements");
        //    var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
        //    ViewBag.HnId = hnId?.ToUpper();
        //    var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
        //    ViewBag.HnName = hnName;
        //    var model = _sessionHelper.GetFromSession<NetworkElementsOverViewModel>(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey);
        //    return View("NetworkElementsOverView", model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> NetworkElementsOverViewAsync()
        //{
        //    await SaveNetworkElements();
        //    ClearNetworkElementSpecificSession();
        //    return RedirectToAction("NetworkDetails", "HeatNetwork");
        //}

        private async Task SaveNetworkElements()
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var elements = _sessionHelper.GetFromSession<List<NetworkElementGroup>>(HttpContext, SessionKeys.SelectedElementsSessionKey);

            var request = new NetworkElements2
            {
                ElementsGroup = elements?.Select(e => new ElementGroup(elementDisplayType: e.ElementDisplayType, count: e.Count)).ToList(),
                NetworkElementStatus = NetworkDetailsStatus.Complete,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                ElementSoaStatus = NetworkDetailsStatus.ReadyToStart
            };

            if (request.ElementsGroup != null)
            {
                request.ElementsGroup = request.ElementsGroup.Select((element, index) =>
                {
                    element.ElementType = NetworkElementHelper.GetNetworkElementIdByType(element.ElementDisplayType.ToString()!);
                    return element;
                }).ToList();
            }

            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var networkType = heatNetworkData?.HeatNetworkType;
            bool hasOwnEc = heatNetworkData?.HasOwnEnergyCentre ?? false;

            if ((Api.Client.Model.HeatNetworkType?)networkType == Api.Client.Model.HeatNetworkType.Communal && hasOwnEc)
            {
                request.ElementsGroup!.Add(new ElementGroup
                {
                    ElementDisplayType = HeatNetworkElementType.EnergyCentre,
                    ElementType = NetworkElementHelper.GetNetworkElementIdByType(HeatNetworkElementType.EnergyCentre.ToString()!),
                    Count = 1
                });
                request.ElementsGroup = request.ElementsGroup.OrderByDescending(e => e.ElementDisplayType == HeatNetworkElementType.EnergyCentre).ToList();
            }
            else if ((Api.Client.Model.HeatNetworkType?)networkType == Api.Client.Model.HeatNetworkType.District && hasOwnEc)
            {
                var isEnergyCentreAdded = request.ElementsGroup!.Any(e => e.ElementDisplayType == HeatNetworkElementType.EnergyCentre);
                if (isEnergyCentreAdded)
                {
                    var energyCentreElement = request.ElementsGroup!.First(e => e.ElementDisplayType == HeatNetworkElementType.EnergyCentre);
                    energyCentreElement.Count = (energyCentreElement.Count ?? 0) + 1;
                }
                else
                {
                    request.ElementsGroup!.Add(new ElementGroup
                    {
                        ElementDisplayType = HeatNetworkElementType.EnergyCentre,
                        ElementType = NetworkElementHelper.GetNetworkElementIdByType(HeatNetworkElementType.EnergyCentre.ToString()!),
                        Count = 1
                    });
                    request.ElementsGroup = request.ElementsGroup.OrderByDescending(e => e.ElementDisplayType == HeatNetworkElementType.EnergyCentre).ToList();
                }
            }
            await _heatNetworkService.UpdateNetworkElements(hnId!, request);
            }

        //private void ClearNetworkElementSpecificSession()
        //{
        //    _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SelectedElementsSessionKey);
        //    _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey);
        //    _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SubstationViewModelKey);
        //    _sessionHelper.ClearFromSession(HttpContext, SessionKeys.DistributionNetworksViewModelKey);
        //    _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);
        //}

        //private List<NetworkElementGroup> GetNetworkElementGroup(List<ElementGroup> elements)
        //{
        //    var neGroups = new List<NetworkElementGroup>();
        //    foreach (var element in elements)
        //    {
        //        var neGroup = new NetworkElementGroup
        //        {
        //            ElementDisplayType = (HeatNetworkElementType)element.ElementDisplayType,
        //            Count = element.Count
        //        };
        //        neGroups.Add(neGroup);
        //    }
        //    return neGroups;
        //}

        private List<ElementGroup> GetElementGroup(List<NetworkElementGroup> neGroups)
        {
            var elements = new List<ElementGroup>();
            foreach (var neGroup in neGroups)
            {
                var element = new ElementGroup
                {
                    ElementDisplayType = (HeatNetworkElementType)neGroup.ElementDisplayType,
                    Count = neGroup.Count
                };
                elements.Add(element);
            }
            return elements;
        }

        //private static string GetElementLabel(string label, int count)
        //{
        //    return label switch
        //    {
        //        "Communal substation (within the communal building)" =>
        //            count == 1
        //                ? "Communal substation (within the communal building)"
        //                : "Communal substations (within the communal building)",

        //        "Communal distribution network" =>
        //            count == 1
        //                ? "Communal distribution network"
        //                : "Communal distribution networks",

        //        "Consumer connection" =>
        //            count == 1
        //                ? "Consumer connection"
        //                : "Consumer connections",

        //        _ => count == 1 ? label : $"{label}s"
        //    };
        //}
    }
}
