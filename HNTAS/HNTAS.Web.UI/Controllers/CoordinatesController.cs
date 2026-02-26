using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HNTAS.Web.UI.Controllers
{
    public class CoordinatesController : Controller
    {
        private readonly ILogger<CoordinatesController> _logger;
        private readonly ISessionHelper _sessionHelper;
        public CoordinatesController(ILogger<CoordinatesController> logger, ISessionHelper sessionHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionHelper = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        [HttpGet]
        public IActionResult ECCoordinates()
        {
            this.ShowBackButton("DoesHNHaveAPostcode", "Address");
            var model = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey) ?? new ECDetailsModel { ECAddressByLatLong = new AddressByLatLongModel() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ECCoordinates(ECDetailsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Try to split and parse the LatitudeLongitude value
            var raw = model.LatitudeLongitude;
            var parts = raw.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            if (parts.Length != 2
                || !decimal.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lat)
                || !decimal.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lon))
            {
                ModelState.AddModelError(nameof(model.LatitudeLongitude),
                    "Enter latitude and longitude in correct format.");
                return View("ECCoordinates", model);
            }

            // Populate nested AddressByLatLongModel
            model.ECAddressByLatLong.Latitude = lat;
            model.ECAddressByLatLong.Longitude = lon;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ECDetailsModelSessionKey, model);
            string previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if (previousStep == "HeatNetwork")
            {
                return RedirectToAction("EnterHNPhase", "HeatNetwork");
            }
            else
            {
                return RedirectToAction("NetworkElementsOverView", "NetworkElements");
            }            
        }
    }
}
