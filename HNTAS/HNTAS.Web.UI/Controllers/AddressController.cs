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
            this.ShowBackButton("DoesHNHaveAPostcode", "Address");
            SearchAddressByPostcodeModel model = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);
            if (model == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel is null.");
                return View("DoesHNHaveAPostcode", "HeatNetwork");
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
                _logger.LogWarning("Malformed address received: {Address}", selectedAddress);
                return BadRequest("Selected address is not in the expected format. It must contain at least street, town/city, and postcode.");
            }

            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = string.Join(",", addressParts.Take(addressParts.Length - 2)) ?? string.Empty,
                TownOrCity = addressParts[addressParts.Length - 2] ?? string.Empty,
                Postalcode = (addressParts[addressParts.Length - 1]).ToUpper() ?? string.Empty,
                Country = "United Kingdom" ?? string.Empty,
                Fulladdress = addressmodel.SelectedFullAddress
            };            
            // Save the new model to session
            _sessionHelper.SaveToSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey, model);

            return RedirectToAction("ConfirmAddress", "Address");
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
