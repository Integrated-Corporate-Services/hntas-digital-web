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
                IsEssential = true,
                Secure = true
            });

            TempData["cookie_banner_action"] = "accepted";

            return LocalRedirect(GetSafeReturnUrl());
        }

        [HttpGet]
        public IActionResult Help()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessibilityStatement()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TermsAndConditions()
        {
            return View();
        }

        #region helperMethods
        private IActionResult SetCookieConsent(string cookieConsent)
        {
            Response.Cookies.Append("cookie_consent", "rejected", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Secure = true
            });

            TempData["cookie_banner_action"] = "rejected";

            return LocalRedirect(GetSafeReturnUrl());
        }

        private string GetSafeReturnUrl()
        {
            const string fallbackUrl = "/";
            var referer = Request.Headers["Referer"].ToString();
            if (string.IsNullOrWhiteSpace(referer))
            {
                return fallbackUrl;
            }
            if (!Uri.TryCreate(referer, UriKind.RelativeOrAbsolute, out var parsedUri))
            {
                return fallbackUrl;
            }
            if (parsedUri.IsAbsoluteUri)
            {
                var sameHost = string.Equals(parsedUri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase);
                var sameScheme = string.Equals(parsedUri.Scheme, Request.Scheme, StringComparison.OrdinalIgnoreCase);
                if (!sameHost || !sameScheme)
                {
                    return fallbackUrl;
                }
                var localPath = parsedUri.PathAndQuery + parsedUri.Fragment;
                return Url.IsLocalUrl(localPath) ? localPath : fallbackUrl;
            }
            var relativeUrl = parsedUri.ToString();
            return Url.IsLocalUrl(relativeUrl) ? relativeUrl : fallbackUrl;
        }
        #endregion
    }
}
