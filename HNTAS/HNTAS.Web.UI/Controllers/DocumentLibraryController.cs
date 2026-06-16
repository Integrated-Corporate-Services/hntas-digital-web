using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class DocumentLibraryController : Controller
    {
        public IActionResult NewBuild()
        {
            return RedirectToAction("Index", "NewBuild");
        }
        public IActionResult ExistingBuild()
        {            
            return RedirectToAction("Index", "ExistingBuild");
        }
        public IActionResult CertifiedBuild()
        {
            this.ShowBackButton("DocumentLibrary", "Home");
            return View();
        }        
    }
}
