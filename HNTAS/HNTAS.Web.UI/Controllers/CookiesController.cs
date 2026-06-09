using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Route("cookies")]
    public class CookiesController : Controller
    {
        [HttpPost("/cookies/accept")]
        [ValidateAntiForgeryToken]
        public IActionResult Accept()
        {
            Response.Cookies.Append("cookie_consent", "accepted", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            TempData["cookie_banner_action"] = "accepted";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost("/cookies/reject")]
        [ValidateAntiForgeryToken]
        public IActionResult Reject()
        {
            Response.Cookies.Append("cookie_consent", "rejected", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            TempData["cookie_banner_action"] = "rejected";

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
