using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class AddressController : Controller
    {
        private readonly ILogger<AddressController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IAddressLookupService _addressLookUpService;
        public AddressController(ILogger<AddressController> logger, ISessionHelper sessionHelper, IAddressLookupService addressLookUpService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionHelper = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
            _addressLookUpService = addressLookUpService ?? throw new ArgumentNullException(nameof(addressLookUpService));
        }

        [HttpGet]
        public IActionResult DoesHNHaveAPostcode()
        {
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork")
            {
                ViewBag.Heading = "Does your heat network have a postcode?";
                ViewBag.Description = "Select one option";
                this.ShowBackButton("EnterHNName", "HeatNetwork");
            }
            else if (previousStep == "EnergyCentre")
            {
                ViewBag.Heading = "Does the primary energy centre for have a postcode?";
                ViewBag.Description = "If the energy centre is located within another building, for example basement, enter the postcode of that building.";
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                ViewBag.HnId = hnId;
                this.ShowBackButton("SelectNetworkElements", "NetworkElements", new {hnId});
            }
            
            var model = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelSessionKey) ?? new DoesHNHaveAPostcodeViewModel { HasPostcode = false };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoesHNHaveAPostcode(DoesHNHaveAPostcodeViewModel model)
        {
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork")
            {
                this.ShowBackButton("EnterHNName", "HeatNetwork");
            }
            else if (previousStep == "EnergyCentre")
            {
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                this.ShowBackButton("SelectNetworkElements", "NetworkElements", new { hnId });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelSessionKey, model);
            if (!model.HasPostcode)
            {                
                return RedirectToAction("ECCoordinates", "Coordinates");
            }
            SearchAddressByPostcodeModel results = await _addressLookUpService.PostcodeLookupAsync(model.Postcode);
            model.Postcode = model.Postcode?.ToUpperInvariant().Trim();
            if (results == null || results.Addresses == null || results.Addresses.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode.");
                return View(model);
            }
            results.Addresses = results.Addresses
                .Select(address => Utility.CapitalizeCommaSeparated(address))
                .ToArray();
            _sessionHelper.SaveToSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey, results);
            
            return RedirectToAction("SearchByPostcodeResults", "Address");
        }

        [HttpGet]
        public IActionResult SearchByPostcodeResults()
        {
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork" || previousStep == "EnergyCentre")
            {
                this.ShowBackButton("DoesHNHaveAPostcode", "Address");
            }            
            else
            {
                this.ShowBackButton("OrganisationAddressByPostcode", "Organisation");
            }

            SearchAddressByPostcodeModel model = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);

            if (model == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel is null.");
                if (previousStep == "heatnetwork")
                {
                    return View("DoesHNHaveAPostcode", "HeatNetwork");
                }
                else if (previousStep == "EnergyCentre")
                {
                    this.ShowBackButton("EnergyCentreAddressCheck", "NetworkElements");
                }
                else
                {
                    return View("OrganisationAddressByPostcode", "Organisation");
                }
            }
            return View(model);
        }

        public IActionResult SelectAddress(string selectedAddress)
        {
            var addressmodel = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);
            if (addressmodel == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel not found in session.");
                return BadRequest("Session expired or invalid. Please try again.");
            }
            addressmodel.SelectedFullAddress = Utility.CapitalizeCommaSeparated(selectedAddress);
            var addressParts = addressmodel.SelectedFullAddress.Split(",");

            if (addressParts.Length < 3)
            {
                _logger.LogError("Malformed address received: {Address}", selectedAddress);
                return BadRequest("Selected address is not in the expected format. It must contain at least street, town/city, and postcode.");
            }

            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = string.Join(",", addressParts.Take(addressParts.Length - 2)).Trim() ?? string.Empty,
                TownOrCity = addressParts[addressParts.Length - 2].Trim() ?? string.Empty,
                Postalcode = (addressParts[addressParts.Length - 1]).ToUpper().Trim() ?? string.Empty,
                Country = "United Kingdom" ?? string.Empty,
                Fulladdress = addressmodel.SelectedFullAddress
            };            
            // Save the new model to session
            _sessionHelper.SaveToSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey, model);
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork" || previousStep == "EnergyCentre")
            {
                return RedirectToAction("ConfirmAddress", "Address");
            }
            else
            {                
                var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);

                if (model == null || organisationModel?.CompanyDetails == null)
                {
                    _logger.LogWarning("Missing session data : Required session data is missing or invalid. Address model and Organisation model with CompanyDetails must be present.");
                    return BadRequest("Missing session data");
                }
                organisationModel.CompanyDetails.RegisteredOfficeAddress = model;
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, organisationModel);
                return RedirectToAction("CompanyConfirm", "Organisation");
            }            
        }

        [HttpGet]
        public IActionResult AddressManualEntry()
        {
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork")
            {
                this.ShowBackButton("EnterHNName", "HeatNetwork");
            }
            else if (previousStep == "EnergyCentre")
            {
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                this.ShowBackButton("SelectNetworkElements", "NetworkElements", new { hnId });
            }
            var model = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey)?.HNAddressByStreet ?? new AddressByStreetOrTownModel { Country = "United Kingdom" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddressManualEntry(AddressByStreetOrTownModel model)
        {
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork")
            {
                this.ShowBackButton("EnterHNName", "HeatNetwork");
            }
            else if (previousStep == "EnergyCentre")
            {
                var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
                this.ShowBackButton("SelectNetworkElements", "NetworkElements", new { hnId });
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var addressParts = new[] { model.StreetAddress, model.TownOrCity, model.Postalcode, model.Country }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            model.Fulladdress = string.Join(", ", addressParts);
            var heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            heatNetworkLocationModel?.HNAddressByStreet = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);
            
            return RedirectToAction("ECCoordinates", "Coordinates");
        }

        [HttpGet]
        public IActionResult ConfirmAddress()
        {
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAddress(AddressByStreetOrTownModel model)
        {
            // Save the confirmed address to session   
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "EnergyCentre")
            {
                return RedirectToAction("SaveEnergyCentreAddressByPostcode", "NetworkElements");
            }
            else
            {
                return RedirectToAction("SaveHNAddressByPostcode", "HeatNetwork");
            }
                
        }
    }
}
