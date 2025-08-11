using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;


namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {

        private readonly ILogger<HeatNetworkController> _logger;
        private readonly IHeatNetworksApi _heatNetworksApi;
        private readonly IUserService _userService;

        public HeatNetworkController(ILogger<HeatNetworkController> logger, IHeatNetworksApi heatNetworksApi, IUserService userService)
        {
            _logger = logger;
            _heatNetworksApi = heatNetworksApi;
            _userService = userService;
        }


        [HttpGet]
        public IActionResult EnterHNLocation() 
        {
            string previousUrl = Request.Headers["Referer"].ToString();
           
            if (previousUrl.Contains("dashboard"))
            {
                Utility.ShowBackButton(this, "UserAccount", "Dashboard");
            }
           
            var heatNetworkLocationModel = SessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            return View("EnterHNLocation", heatNetworkLocationModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnterHNLocation(HeatNetworkLocationModel model)
        {
            string previousUrl = Request.Headers["Referer"].ToString();

            if (previousUrl.Contains("dashboard"))
            {
                Utility.ShowBackButton(this, "UserAccount", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (!string.IsNullOrWhiteSpace(model.HeatNetworkLocation) && !model.HeatNetworkLocation.Contains("https://what3words.com/"))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkLocation), "Invalid url. Please enter the correct url.");
                return View(model);
            }
            else
            {
                // Extract the part after "https://what3words.com/"
                var prefix = "https://what3words.com/";
                var urlPart = model.HeatNetworkLocation.Substring(prefix.Length);

                // Validate: 3 words, joined by 2 dots, no whitespace
                // Regex: ^([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)$
                if (string.IsNullOrWhiteSpace(urlPart) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(urlPart, @"^([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)$"))
                {
                    ModelState.AddModelError(nameof(model.HeatNetworkLocation), "Invalid url. Please enter the correct url.");
                }
            }
            
            

            SessionHelper.SaveToSession<HeatNetworkLocationModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkLocationModelKey, model);

            return RedirectToAction("EnterHNName");
        }

        [HttpGet]
        public IActionResult EnterHNName()
        {
            Utility.ShowBackButton(this, "EnterHNLocation", "HeatNetwork");
            var heatNetworkLocationModel = SessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            return View(heatNetworkLocationModel);
        }

        [HttpPost]
        public IActionResult EnterHNName(HeatNetworkNameModel model)
        {
            Utility.ShowBackButton(this, "EnterHNLocation", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                Utility.ShowBackButton(this, "EnterHNLocation", "HeatNetwork");
                return View(model);
            }
            else if (!string.IsNullOrWhiteSpace(model.HeatNetworkName) && model.HeatNetworkName.Length > 100)
            {
                ModelState.AddModelError(nameof(model.HeatNetworkName), "The heat network name cannot exceed 100 characters.");
                return View(model);
            }
            
            SessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkNameModelKey, model);
            return RedirectToAction("CheckYourAnswers");
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            ViewBag.ShowBackButton = false;

            var checkAnswersModel = new CheckYourAnswersHeatNetworkModel
            {
                HeatNetworkNameModel = SessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkNameModelKey),
                HeatNetworkLocationModel = SessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkLocationModelKey),
                ConfirmedDeclaration = false
            };

            return View(checkAnswersModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(CheckYourAnswersHeatNetworkModel viewModel)
        {
            
            viewModel.HeatNetworkNameModel = SessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkNameModelKey);
            viewModel.HeatNetworkLocationModel = SessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkLocationModelKey);

            ModelState.Remove(nameof(viewModel.HeatNetworkNameModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkLocationModel));

            if (!ModelState.IsValid)
            {
                return View("CheckYourAnswers", viewModel);
            }

            try { 
                var model = new HeatNetwork
                {
                  Name = viewModel.HeatNetworkNameModel.HeatNetworkName,
                  Location = viewModel.HeatNetworkLocationModel.HeatNetworkLocation,
                };

                var response = await _heatNetworksApi.ApiHeatNetworksAddHeatNetworkPostAsync(model);

                if (response.IsCreated)
                {
                    var hnmodel = response.Created();
                    TempData["Confirmation_HN_Id"] = hnmodel.HnId;

                    await _userService.UpdateUserHeatNetworkId(SessionHelper.GetFromSession<string>(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey), hnmodel.HnId);
                    _logger.LogInformation("Heat network created successfully with ID: {Id}", hnmodel.Id);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error submitting heat network answers.");
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later.";
                return View("CheckYourAnswers", viewModel);
            }
            SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, false);

            return RedirectToAction("Confirmation", "HeatNetwork");
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            var userResponse = await _userService.GetUserById(SessionHelper.GetFromSession<string>(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey));

            ViewBag.CompanyName = userResponse.Organisation?.Name;
            ViewBag.ContactName = userResponse.FullName;
            ViewBag.HNId = TempData["Confirmation_HN_Id"] as string;
            return View("Confirmation");
        }
    }
}
