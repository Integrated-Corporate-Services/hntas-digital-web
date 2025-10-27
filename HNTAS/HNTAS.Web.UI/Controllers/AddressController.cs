using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace HNTAS.Web.UI.Controllers
{
    public class AddressController : Controller
    {
        private readonly AddressLookupService _addressLookUpService;
        private readonly ILogger<AddressController> _logger;

        private const string what3wordsurlModelKey = "what3wordsurl";

        public AddressController(ILogger<AddressController> logger, AddressLookupService addressLookupService)
        {
            _logger = logger;
            _addressLookUpService = addressLookupService;
        }

        [HttpGet]
        public IActionResult ManualAddressEntry()
        {
            ModelState.Clear();
            return View("ManualAddressEntry");
        }

        [HttpPost]
        public IActionResult ManualAddressEntry(AddressByStreetOrTownModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Postalcode) &&
                !Regex.IsMatch(model.Postalcode.Trim().ToUpper(), "^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|[A-HK-Y][0-9]{1,2}|[0-9][A-HJKS-UW]|[A-HK-Y][0-9][ABEHMNPRV-Y]) ?[0-9][ABD-HJLNP-UW-Z]{2})$"))
            {
                ModelState.AddModelError(nameof(model.Postalcode), "Please enter a valid UK postcode.");
            }

            if (!ModelState.IsValid)
            {
                // Return the view with the model to preserve user input and show errors
                return View("ManualAddressEntry", model);
            }

            // Join non-empty fields with commas
            var addressParts = new[] { model.StreetAddress, model.TownOrCity, model.Postalcode, model.Country }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            model.Fulladdress = string.Join(", ", addressParts);

            return View("SelectAddressInputMethod");
        }

        [HttpGet]
        public IActionResult AddressLookUp()
        {
            ModelState.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddressLookUp(string postcode)
        {
            if (string.IsNullOrEmpty(postcode))
            {
                return View("AddressLookUp");
            }
            if (!string.IsNullOrWhiteSpace(postcode) &&
                !Regex.IsMatch(postcode.Trim().ToUpper(), "^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|[A-HK-Y][0-9]{1,2}|[0-9][A-HJKS-UW]|[A-HK-Y][0-9][ABEHMNPRV-Y]) ?[0-9][ABD-HJLNP-UW-Z]{2})$"))
            {
                ModelState.Remove("Postcode");
                ModelState.AddModelError("postcode", "Please enter a valid UK postcode.");
                return View("AddressLookUp");
            }

            try
            {
                var model = await _addressLookUpService.PostcodeLookupAsync(postcode);
                if (model == null || model.Addresses == null || model.Addresses.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode.");
                }
                return View("SearchResults", model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Unable to retrieve address data.");
                return View("SearchResults");
            }
        }

        [HttpGet]
        public IActionResult SearchResults(SearchAddressByPostcodeModel model)
        {
            this.ShowBackButton("AddressLookUp");
            return View(model);
        }

        [HttpGet]
        public IActionResult SelectAddress(string fulladdress, SearchAddressByPostcodeModel addressmodel)
        {            
            addressmodel.SelectedFullAddress = fulladdress;
            Console.WriteLine(addressmodel.SelectedFullAddress);
            return View("SelectAddressInputMethod");
        }        
    }
}
