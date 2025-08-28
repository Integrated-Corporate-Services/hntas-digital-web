using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;


namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {

        private readonly ILogger<HeatNetworkController> _logger;
        private readonly IHeatNetworksApi _heatNetworksApi;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;

        public HeatNetworkController(ILogger<HeatNetworkController> logger, IHeatNetworksApi heatNetworksApi, IUserService userService, ISessionHelper sessionHelper)
        {
            _logger = logger;
            _heatNetworksApi = heatNetworksApi;
            _userService = userService;
            _sessionHelper = sessionHelper;
        }


        [HttpGet]
        public IActionResult EnterHNName()
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            return View(heatNetworkNameModel);
        }

        [HttpPost]
        public IActionResult EnterHNName(HeatNetworkNameModel model)
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (!string.IsNullOrWhiteSpace(model.HeatNetworkName) && model.HeatNetworkName.Length > 100)
            {
                ModelState.AddModelError(nameof(model.HeatNetworkName), "The heat network name cannot exceed 100 characters.");
                return View(model);
            }

            _sessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey, model);
            return RedirectToAction("EnterHNLocation");
        }


        [HttpGet]
        public IActionResult EnterHNLocation()
        {
            this.ShowBackButton("EnterHNName", "HeatNetwork");
            var heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            ViewBag.HNName = heatNetworkNameModel.HeatNetworkName;
            return View("EnterHNLocation", heatNetworkLocationModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnterHNLocation(HeatNetworkLocationModel model)
        {
            this.ShowBackButton("EnterHNName", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (!string.IsNullOrWhiteSpace(model.HeatNetworkLocation) && !model.HeatNetworkLocation.StartsWith("https://what3words.com/"))
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

            _sessionHelper.SaveToSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey, model);
            return RedirectToAction("EnterHNPhase");
        }

        [HttpGet]
        public IActionResult EnterHNPhase()
        {
            this.ShowBackButton("EnterHNLocation", "HeatNetwork");
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey) ?? new HeatNetworkPhaseModel();
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            ViewBag.HNName = heatNetworkNameModel.HeatNetworkName;
            return View("EnterHNPhase", heatNetworkPhaseModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnterHNPhase(HeatNetworkPhaseModel model)
        {
            this.ShowBackButton("EnterHNLocation", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (string.IsNullOrWhiteSpace(model.HeatNetworkPhase))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase.");
                return View(model);
            }
            else
            {
                _sessionHelper.SaveToSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model);
                switch (model.HeatNetworkPhase)
                {
                    case "design":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers");
                    case "construction":
                        return RedirectToAction("HasElementBeenRegistered");
                    case "operation":
                        return RedirectToAction("HNInOperation");
                    default:
                        ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase.");
                        return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult HasElementBeenRegistered()
        {
            this.ShowBackButton("EnterHNPhase", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<HasElementBeenRegisteredModel>(HttpContext, SessionKeys.HasElementBeenRegisteredModelKey) ?? new HasElementBeenRegisteredModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HasElementBeenRegistered(HasElementBeenRegisteredModel model)
        {
            this.ShowBackButton("EnterHNPhase", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (string.IsNullOrWhiteSpace(model.HasElementBeenRegistered))
            {
                ModelState.AddModelError(nameof(model.HasElementBeenRegistered), "Please select an option.");
                return View(model);
            }
            else
            {
                _sessionHelper.SaveToSession<HasElementBeenRegisteredModel>(HttpContext, SessionKeys.HasElementBeenRegisteredModelKey, model);
                switch (model.HasElementBeenRegistered)
                {
                    case "yes":
                        return RedirectToAction("HasPlanningApplicationBeenSubmitted");
                    case "no":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers");
                    default:
                        ModelState.AddModelError(nameof(model.HasElementBeenRegistered), "Please select an option.");
                        return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult HasPlanningApplicationBeenSubmitted()
        {
            this.ShowBackButton("HasElementBeenRegistered", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<HasPlanningApplicationBeenSubmittedModel>(HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey) ?? new HasPlanningApplicationBeenSubmittedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HasPlanningApplicationBeenSubmitted(HasPlanningApplicationBeenSubmittedModel model)
        {
            this.ShowBackButton("HasElementBeenRegistered", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (string.IsNullOrWhiteSpace(model.HasPlanningApplicationBeenSubmitted))
            {
                ModelState.AddModelError(nameof(model.HasPlanningApplicationBeenSubmitted), "Please select an option.");
                return View(model);
            }
            else
            {
                _sessionHelper.SaveToSession<HasPlanningApplicationBeenSubmittedModel>(HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey, model);
                switch (model.HasPlanningApplicationBeenSubmitted)
                {
                    case "yes":
                        //store pathway as 3, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "3" });
                        return RedirectToAction("CheckYourAnswers");
                    case "no":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers");
                    default:
                        ModelState.AddModelError(nameof(model.HasPlanningApplicationBeenSubmitted), "Please select an option.");
                        return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult HNInOperation()
        {
            this.ShowBackButton("EnterHNPhase", "HeatNetwork");
            return View();
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            ViewBag.ShowBackButton = false;

            var checkAnswersModel = new CheckYourAnswersHeatNetworkModel
            {
                HeatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey),
                HeatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey),
                HeatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey),
                HasElementBeenRegisteredModel = _sessionHelper.GetFromSession<HasElementBeenRegisteredModel>(HttpContext, SessionKeys.HasElementBeenRegisteredModelKey) ?? null,
                HasPlanningApplicationBeenSubmittedModel = _sessionHelper.GetFromSession<HasPlanningApplicationBeenSubmittedModel>(HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey) ?? null,
                PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey) ?? new PathwayModel() { Pathway = "1" },
                ConfirmedDeclaration = false
            };


            return View(checkAnswersModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(CheckYourAnswersHeatNetworkModel viewModel)
        {

            viewModel.HeatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            viewModel.HeatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            viewModel.PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey);

            ModelState.Remove(nameof(viewModel.HeatNetworkNameModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkLocationModel));
            ModelState.Remove(nameof(viewModel.PathwayModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkPhaseModel));

            if (!ModelState.IsValid)
            {
                return View("CheckYourAnswers", viewModel);
            }

            try
            {
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

                    await _userService.UpdateUserHeatNetworkId(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey), hnmodel.HnId);
                    _logger.LogInformation("Heat network created successfully with ID: {Id}", hnmodel.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting heat network answers.");
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later.";
                return View("CheckYourAnswers", viewModel);
            }
            _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);

            return RedirectToAction("Confirmation", "HeatNetwork");
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            var userResponse = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));

            ViewBag.CompanyName = userResponse.Organisation?.Name;
            ViewBag.ContactName = userResponse.FullName;
            ViewBag.HNId = TempData["Confirmation_HN_Id"] as string;
            ViewBag.HNName = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey)?.HeatNetworkName;
            return View("Confirmation");
        }

        [HttpGet]
        public async Task<IActionResult> Details([FromQuery] string hnid)
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            // get user details
            var response = await _heatNetworksApi.ApiHeatNetworksHnIdGetAsync(hnid?.ToUpper());

            if (response.IsOk)
            {
                var responseModel = response.Ok();

                var model = new HNDetailsViewModel
                {
                    Name = responseModel?.Name,
                    LocationUrl = responseModel?.Location,
                    OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName),
                    PathWay = "",
                    UHNID = responseModel?.HnId
                };

                return View(model);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult SubmitDetails(HNDetailsViewModel model)
        {
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, model.UHNID);
            return RedirectToAction("SOAIntro", "SOA");
        }

    }
}
