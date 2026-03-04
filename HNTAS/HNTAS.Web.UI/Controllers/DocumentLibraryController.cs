using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class DocumentLibraryController : Controller
    {
        public IActionResult NewBuild()
        {
            this.ShowBackButton("DocumentLibrary", "Home");
            return View();
        }
        public IActionResult ExistingBuild()
        {
            this.ShowBackButton("DocumentLibrary", "Home");
            return View();
        }
        public IActionResult CertifiedBuild()
        {
            this.ShowBackButton("DocumentLibrary", "Home");
            return View();
        }
        public IActionResult EnergyCentre()
        {
            this.ShowBackButton("NewBuild");
            return View();
        }
        public IActionResult Substation()
        {
            this.ShowBackButton("NewBuild");
            return View();
        }
        public IActionResult DistrictDistributionNetwork()
        {
            this.ShowBackButton("NewBuild");
            return View();
        }
        public IActionResult CommunalDistributionNetwork()
        {
            this.ShowBackButton("NewBuild");
            return View();
        }
        public IActionResult ConsumerConnection()
        {
            this.ShowBackButton("NewBuild");
            return View();
        }
        public IActionResult ConsumerHeatSystem()
        {
            this.ShowBackButton("NewBuild");
            return View();
        }        
    }
}
