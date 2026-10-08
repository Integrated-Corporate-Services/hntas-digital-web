using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class EndOfJourney : Controller
    {
        public EndOfJourney() { }

        [HttpGet]
        public IActionResult UserIsNotRP()
        {
            this.ShowBackButton("AreYouTheRP", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult HNIsOperationalRegisterLater()
        {
            this.ShowBackButton("IsYourOrgWorkingOnANewHN", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult HNNotINEnglandScotlandWales()
        {
            this.ShowBackButton("IsHNLocatedInEnglandScotlandWales", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult LessThan10Dwellings()
        {
            this.ShowBackButton("HowManyDwellingsIncluded", "HeatNetworkEligibility");
            return View();
        }
    }
}
