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
    }
}
