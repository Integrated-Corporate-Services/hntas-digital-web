using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;
using Element = HNTAS.Web.UI.Models.NetworkElements.Element;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class NetworkElementsController : Controller
    {
        private readonly ILogger<NetworkElementsController> _logger;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IOrganisationService _organisationService;
        private readonly IAddressLookupService _addressLookUpService;

        public NetworkElementsController(ILogger<NetworkElementsController> logger, IHeatNetworkService heatNetworkService, IUserService userService, ISessionHelper sessionHelper, IOrganisationService organisationService, IAddressLookupService addressLookupService)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _organisationService = organisationService;
            _addressLookUpService = addressLookupService;
        }

        [HttpGet]
        public IActionResult SelectNetworkElements([FromQuery] string hnId)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            hnId = hnId ?? _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId.ToUpper();
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnId?.ToUpper());
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, hnName);
            var model = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);

            if (model != null)
            {
                return View(model);
            }
            model = new NetworkElementViewModel()
            {
                ElementOptions = Utility.GetDefaultNetworkElementOptions()
            };
            
            var networkType = _sessionHelper.GetFromSession<HeatNetworkType>(HttpContext, SessionKeys.HeatNetworkTypeSessionKey);


            if (networkType == HeatNetworkType.CommunalHeatNetwork)
            {                 
                model.ElementOptions = model.ElementOptions
                    .Where(e => e.Id == HeatNetworkElementDisplayType.EnergyCentre || e.Id == HeatNetworkElementDisplayType.ConsumerHeatSystems)
                    .ToList();
            }            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectNetworkElements(NetworkElementViewModel model)
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            model.ElementOptions = Utility.GetDefaultNetworkElementOptions();

            var networkType = _sessionHelper.GetFromSession<HeatNetworkType>(HttpContext, SessionKeys.HeatNetworkTypeSessionKey);

            if (networkType == HeatNetworkType.CommunalHeatNetwork)
            {
                model.ElementOptions = model.ElementOptions
                    .Where(e => e.Id == HeatNetworkElementDisplayType.EnergyCentre || e.Id == HeatNetworkElementDisplayType.ConsumerHeatSystems)
                    .ToList();
            }

            if (!ModelState.IsValid)
            {
                return View("SelectNetworkElements", model);
            }
            var elements = new List<Element>();

            foreach (var selectedId in model.SelectedElementIds)
            {
                var ele = new Element
                {
                    Type = selectedId,
                    Count = model.ElementCounts.TryGetValue(selectedId, out var cnt) ? cnt : null
                };
                elements.Add(ele);
                if ((selectedId != HeatNetworkElementDisplayType.EnergyCentre) && (!model.ElementCounts.TryGetValue(selectedId, out var count) || count == null || count <= 0))
                {
                    var element = Utility.GetDefaultNetworkElementOptions().FirstOrDefault(x => x.Id == selectedId);
                    if (element == null)
                    {
                        return BadRequest();
                    }
                    ModelState.AddModelError($"ElementCounts[{selectedId}]", $"Enter number of {element.Label}.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("SelectNetworkElements", model);
            }
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.PreviousStepKey, "EnergyCentre");
            _sessionHelper.SaveToSession<List<Element>>(HttpContext, SessionKeys.SelectedElementsSessionKey, elements);
            if (model.SelectedElementIds.Contains(HeatNetworkElementDisplayType.EnergyCentre))
            {
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey, model);
                var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                var latlong = heatNetworkData?.EcDetails;
                var address = heatNetworkData?.Address;
                var ecDetailsModel = new ECDetailsModel
                {
                    LatitudeLongitude = latlong != null ? $"{latlong.Latitude},{latlong.Longitude}" : null
                };
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.ECDetailsModelSessionKey, ecDetailsModel);
                if (address != null)
                {
                    var addressByStreetOrTownModel = (AddressByStreetOrTownModel)address!;
                    _sessionHelper.SaveToSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey, addressByStreetOrTownModel);
                    return RedirectToAction("ConfirmAddress", "Address");
                }

                return RedirectToAction("DoesHNHaveAPostcode", "Address");
            }
            await SaveNetworkElements(new NetworkElementsModel());
            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }       

        [HttpGet]
        public IActionResult SaveEnergyCentreAddressByPostcode()
        {
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey) ?? new AddressByStreetOrTownModel();
            if (model == null)
            {
                return BadRequest("Missing session data");
            }
            var energyCentreLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.EnergyCentreLocationModelKey) ?? new HeatNetworkLocationModel { HNAddressByStreet = new AddressByStreetOrTownModel() };
            energyCentreLocationModel.HNAddressByStreet = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, energyCentreLocationModel);
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.PreviousStepKey, "EnergyCentre");
            return RedirectToAction("ECCoordinates", "Coordinates");
        }

        [HttpGet]
        public IActionResult NetworkElementsOverView()
        {
            this.ShowBackButton("ECCoordinates", "Coordinates");

            var heatNetworkLocation = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            var ecDetails = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey);           
            var selectedElements = _sessionHelper.GetFromSession<List<Element>>(HttpContext, SessionKeys.SelectedElementsSessionKey) ?? new List<Element>();
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var model = new NetworkElementsModel
            {
                Elements = selectedElements,
                HeatNetworkAddressModel = heatNetworkLocation?.HNAddressByStreet ?? new AddressByStreetOrTownModel(),
                ECDetailsModel = ecDetails,                
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NetworkElementsOverView(NetworkElementsModel viewModel)
        {            
            await SaveNetworkElements(viewModel);
            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }

        private async Task SaveNetworkElements(NetworkElementsModel viewModel)
        {
            var hnLocation = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            viewModel.HeatNetworkAddressModel = hnLocation?.HNAddressByStreet ?? null;
            viewModel.ECDetailsModel = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey) ?? null;

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var elements = _sessionHelper.GetFromSession<List<Element>>(HttpContext, SessionKeys.SelectedElementsSessionKey);

            ModelState.Remove(nameof(viewModel.HeatNetworkAddressModel));
            ModelState.Remove(nameof(viewModel.ECDetailsModel));            

            var hnAddress = viewModel?.HeatNetworkAddressModel;

            double? latitude = null;
            double? longitude = null;
            if (viewModel?.ECDetailsModel?.ECAddressByLatLong != null)
            {
                latitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Latitude;
                longitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Longitude;
            }

            ECDetails2? ecDetails = (latitude.HasValue || longitude.HasValue)
                ? new ECDetails2(latitude: latitude, longitude: longitude)
                : null;

            var address = viewModel.HeatNetworkAddressModel != null ? new RegisteredAddress(
                    addressLine1: hnAddress?.StreetAddress?.Trim(),
                    postcode: hnAddress?.Postalcode?.Trim(),
                    addressLine2: default,
                    town: hnAddress?.TownOrCity?.Trim(),
                    county: default,
                    country: hnAddress?.Country?.Trim()
                ) : null;

            var request = new NetworkElements2
            {
                Elements = elements?.Select(e => new Api.Client.Model.Element(type: e.Type, count: e.Count)).ToList(),
                NetworkElementStatus = NetworkDetailsStatus.Complete,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                ElementSoaStatus = NetworkDetailsStatus.ReadyToStart
            };            

            if (request.Elements != null)
            {
                request.Elements = request.Elements.Select((element, index) =>
                {                    
                    element.ElementId = (index + 1).ToString("D5");
                    element.ElementType = Utility.GetNetworkElementIdByType(element.Type.ToString()!);

                    if (element.Type == HeatNetworkElementDisplayType.EnergyCentre)
                    {
                        element.EcDetails = ecDetails;
                        element.Address = address;
                    }

                    return element;
                }).ToList();
            }
            await _heatNetworkService.UpdateNetworkElements(hnId!, request);
        }
    }
}
