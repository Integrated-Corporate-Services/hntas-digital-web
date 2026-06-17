using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HelpController : Controller
    {
        [HttpGet]
        public IActionResult PrivacyNotice()
        {
            return View();
        }
    }
}
