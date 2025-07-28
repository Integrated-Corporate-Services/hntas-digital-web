using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult UserAccount()
        {
            // access API to retrieve org name, all the heat networks registered
            //var organisationName = SessionHelper.GetFromSession<>
            return View();
        }
    }
}
