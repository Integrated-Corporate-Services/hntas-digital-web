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
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var model = new NetworkElementViewModel()
            {
                ElementOptions = Utility.GetDefaultNetworkElementOptions()
            };
            ViewBag.HnId = hnId;
            //ViewBag.selectedCharacteristics = heatNetworkData.NetworkCharacteristics;
            ViewBag.selectedCharacteristics = "Communal Heat Network";

            //if (ViewBag.selectedCharacteristics == "Communal Heat Network")
            //{
            //    // keep HeatNetworkElementType.EnergyCentre and HeatNetworkElementType.ConsumerHeatSystems from the list of options 
            //    model.ElementOptions = model.ElementOptions
            //        .Where(e => e.Id == HeatNetworkElementType.EnergyCentre || e.Id == HeatNetworkElementType.ConsumerHeatSystems)
            //        .ToList();
            //}

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnId);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, hnName);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SelectNetworkElements(NetworkElementViewModel model)
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            model.ElementOptions = Utility.GetDefaultNetworkElementOptions();

            if (!ModelState.IsValid)
            {
                return View("SelectNetworkElements", model);
            }
            var elements = new List<Element>();
            // Custom validation: ensure quantity is entered for each selected element
            foreach (var selectedId in model.SelectedElementIds)
            {
                var ele = new Element
                {
                    Type = selectedId,
                    Count = model.ElementCounts.TryGetValue(selectedId, out var cnt) ? cnt : null
                };
                elements.Add(ele);
                if ((selectedId != HeatNetworkElementType.EnergyCentre) && (!model.ElementCounts.TryGetValue(selectedId, out var count) || count == null || count <= 0))
                {
                    var element = Utility.GetDefaultNetworkElementOptions().FirstOrDefault(x => x.Id == selectedId);
                    if (element == null)
                    {
                        return BadRequest();
                    }
                    ModelState.AddModelError($"ElementCounts[{selectedId}]", $"Enter number of {element.Label}.");
                }
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnId);
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.PreviousStepKey, "EnergyCentre");
            _sessionHelper.SaveToSession<List<Element>>(HttpContext, SessionKeys.SelectedElementsSessionKey, elements);
            if (model.SelectedElementIds.Contains(HeatNetworkElementType.EnergyCentre))
            {
                var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
                //_sessionHelper.SaveToSession<ECDetails>(HttpContext, SessionKeys.ECDetailsModelSessionKey, new ECDetailsModel { LatitudeLongi });
                //var latLong = _sessionHelper.GetFromSession<ECDetailsM>(HttpContext, SessionKeys.ECDetailsModelSessionKey) ?? new ECDetailsModel { ECAddressByLatLong = new AddressByLatLongModel() };
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

            return RedirectToAction("DoesHNHaveAPostcode", "Address");

            
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

            var model = new NetworkElementsOverviewModel
            {
                Elements = selectedElements,
                HeatNetworkAddressModel = heatNetworkLocation?.HNAddressByStreet ?? new AddressByStreetOrTownModel(),
                ECDetailsModel = ecDetails,                
            };

            //_sessionHelper.SaveToSession<NetworkElementsOverviewModel>(HttpContext, "CheckYourAnswersNetworkElementsModel", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NetworkElementsOverView(NetworkElementsOverviewModel viewModel)
        {            
            var hnLocation = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            viewModel.HeatNetworkAddressModel = hnLocation?.HNAddressByStreet ?? null;
            viewModel.ECDetailsModel = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey);
            
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var elements = _sessionHelper.GetFromSession<List<Element>>(HttpContext, SessionKeys.SelectedElementsSessionKey);

            ModelState.Remove(nameof(viewModel.HeatNetworkAddressModel));
            ModelState.Remove(nameof(viewModel.ECDetailsModel));

            if (!ModelState.IsValid)
            {
                return View("NetworkElementsOverView", viewModel);
            }

            if (userId == null)
            {
                TempData["ErrorMessage"] = "An error occurred while saving network elements details. Please try again later.";
                return View("NetworkElementsOverView", viewModel);
            }

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

            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }
    }
}
