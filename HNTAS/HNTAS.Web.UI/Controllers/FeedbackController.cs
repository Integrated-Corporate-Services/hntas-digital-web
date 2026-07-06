using HNTAS.Api.Client.Api;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackApi _feedbackApi;
        public FeedbackController(IFeedbackApi feedbackApi)
        {
            _feedbackApi = feedbackApi;
        }

        [HttpGet]
        public IActionResult Index()
        {
            this.ShowBackButton("StartPage", "Home");
            ModelState.Clear();
            return View(new FeedbackFormModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FeedbackFormModel model)
        {
            this.ShowBackButton("StartPage", "Home");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var response = await _feedbackApi.ApiFeedbackPostAsync(new Api.Client.Model.CreateFeedbackRequest { SatisfactionLevel = model.SatisfactionLevel, FeedbackText = model.Feedback });
                return RedirectToAction("FeedbackReceived");
            }
            catch (Api.Client.Client.ApiException ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while submitting feedback. Please try again.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult FeedbackReceived()
        {
            this.ShowBackButton("Index");
            return View();
        }

    }
}
