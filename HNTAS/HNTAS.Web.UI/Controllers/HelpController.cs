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

        [HttpGet]
        public IActionResult Cookies()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cookies(string cookieConsent)
        {
            SetCookieConsent(cookieConsent);
            TempData["cookie_banner_action"] = cookieConsent;
            return RedirectToAction("Cookies");
        }        

        private void SetCookieConsent(string cookieConsent)
        {
            Response.Cookies.Append("cookie_consent", cookieConsent, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Secure = true
            });

            if (cookieConsent == "No")
            {
                Response.Cookies.Delete("_ga");
                Response.Cookies.Delete("_ga_NGJT0GGBSZ");
            }
        }        
    }
}
