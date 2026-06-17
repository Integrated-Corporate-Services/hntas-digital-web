using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult SignIn(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            // You can also set specific VectorsOfTrust on a per-request basis if needed
            // properties.SetVectorsOfTrust(new[] { "Cl.Cm" }); 
            return Challenge(properties, authenticationSchemes: OneLoginDefaults.AuthenticationScheme);
        }

        [Authorize] // Requires the user to be authenticated
        public async Task<IActionResult> SignOut()
        {
            // Clear your application's session data
            HttpContext.Session.Clear();

            // Sign out from your application's cookie scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign out from GOV.UK One Login
            var useGovUkSimulator = Environment.GetEnvironmentVariable("SIMULATOR_PROP4");

            if (!string.IsNullOrEmpty(useGovUkSimulator) && useGovUkSimulator.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return SignOut(new AuthenticationProperties(), "GovUkSimulator");
            }
            else
            {
                return SignOut(new AuthenticationProperties(), OneLoginDefaults.AuthenticationScheme);
            }


        }

        // This action will be called after successful authentication by One Login
        // Its path should match the CallbackPath configured in AddOneLogin options
        [AllowAnonymous]
        [Route("/onelogin-callback")]
        public IActionResult OneLoginCallback()
        {            
            // The middleware handles the token exchange and setting the cookie.
            // You can redirect the user to their intended destination or a dashboard.
            return RedirectToAction("Index", "Home");
        }

        // This action will be called after successful logout by One Login
        // Its path should match the SignedOutCallbackPath configured in AddOneLogin options
        [AllowAnonymous]
        [Route("/onelogin-logout-callback")]
        public IActionResult OneLoginLogoutCallback()
        {
            // The middleware has already cleared the One Login session.
            // Redirect to a public page after logout.
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // This forces your Home/Error logic to run with code 403
            return RedirectToAction("Error", "Home", new { code = 403 });
        }
    }
}
