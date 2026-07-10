using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class FooterLinksController : Controller
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