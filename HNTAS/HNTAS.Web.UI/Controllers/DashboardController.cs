using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult UserAccount()
        {

            return View();
        }
    }
}
