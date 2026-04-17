using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class ExistingBuildController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Overview()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult Milestone2()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult Milestone3A()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult Milestone3B()
        {
            this.ShowBackButton("Index");
            return View();
        }
        public IActionResult Milestone4()
        {
            this.ShowBackButton("Index");
            return View();
        }
    }
}
