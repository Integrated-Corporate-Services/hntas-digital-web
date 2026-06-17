using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class FeedbackController : Controller
    {
        public IActionResult Index()
        {
            this.ShowBackButton("StartPage", "Home");
            return View();
        }
    }
}
