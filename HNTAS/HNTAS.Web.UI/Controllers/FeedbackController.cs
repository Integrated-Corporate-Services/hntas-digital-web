using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class FeedbackController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            this.ShowBackButton("StartPage", "Home");
            return View(new FeedbackFormModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FeedbackFormModel model)
        {
            this.ShowBackButton("StartPage", "Home");
            if (!ModelState.IsValid)
            {                
                return View(model);
            }
            // Process the feedback (e.g., save to database, send email, etc.)
            // For now, we will just redirect to a thank you page.
            return View(new FeedbackFormModel());
        }
    }
}
