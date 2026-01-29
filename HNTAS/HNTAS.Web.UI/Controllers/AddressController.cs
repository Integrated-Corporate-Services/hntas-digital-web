using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class AddressController : Controller
    {
        private readonly ILogger<AddressController> _logger;
        private readonly ISessionHelper _sessionHelper;
        public AddressController(ILogger<AddressController> logger, ISessionHelper sessionHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionHelper = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        [HttpGet]
        public IActionResult SearchByPostcodeResults()
        {
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork")
            {
                this.ShowBackButton("DoesHNHaveAPostcode", "HeatNetwork");
            }else
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
            if (previousStep == "HeatNetwork")
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
        public IActionResult ConfirmAddress()
        {
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey);
            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmAddress(AddressByStreetOrTownModel model)
        {            
            // Save the confirmed address to session            
            return RedirectToAction("SaveHNAddressByPostcode", "HeatNetwork");
        }
    }
}
