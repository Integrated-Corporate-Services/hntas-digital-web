using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class NewBuildController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult EnergyCentre()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult Substation()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult DistrictDistributionNetwork()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult CommunalDistributionNetwork()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult ConsumerConnection()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult ConsumerHeatSystem()
        {
            this.ShowBackButton("Index");
            return View();
        }
    }
}
