using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class DocumentLibraryController : Controller
    {
        public IActionResult NewBuild()
        {
            return View();
        }
        public IActionResult ExistingBuild()
        {
            return View();
        }
        public IActionResult CertifiedBuild()
        {
            return View();
        }
        public IActionResult EnergyCentre()
        {
            return View();
        }
        public IActionResult Substation()
        {
            return View();
        }
        public IActionResult DistrictDistributionNetwork()
        {
            return View();
        }
        public IActionResult CommunalDistributionNetwork()
        {
            return View();
        }
        public IActionResult ConsumerConnection()
        {
            return View();
        }
        public IActionResult ConsumerHeatSystem()
        {
            return View();
        }        
    }
}
