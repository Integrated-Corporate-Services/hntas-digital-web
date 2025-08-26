using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace HNTAS.Web.UI.Controllers
{
    public class ContributorController : Controller
    {
        private readonly IUserService _iUserService;
        private readonly ILogger<ContributorController> _logger;
        private readonly ISessionHelper _sessionHelper;
        public ContributorController(IUserService iUserService, ILogger<ContributorController> logger, ISessionHelper sessionHelper)
        {
            _iUserService = iUserService;
            _logger = logger;
            _sessionHelper = sessionHelper;
        }

        #region No action Pages
        [HttpGet]
        public IActionResult YouHaveDeclined()
        {
            this.ShowBackButton("YouHaveBeenInvited", "Contributor");
            return View();
        }

        #endregion

        #region User input Pages

        [HttpGet]
        public IActionResult YouHaveBeenInvited()
        {
            var model = _sessionHelper.GetFromSession<YouHaveBeenInvitedModel>(HttpContext, SessionKeys.YouHaveBeenInvitedModelKey) ?? new YouHaveBeenInvitedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult YouHaveBeenInvited(YouHaveBeenInvitedModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            switch (model.AcceptInvitation) {
                case "accept":
                    return RedirectToAction("StartPage", "Contributor");
                case "decline":
                    return RedirectToAction("YouHaveDeclined", "Contributor");
                default:
                    ModelState.AddModelError(nameof(model.AcceptInvitation), "Please select a valid option.");
                    return View(model);
            }
        }



        [HttpGet]
        public IActionResult StartPage() {
            this.ShowBackButton("YouHaveBeenInvited", "Contributor");
            return View();
        }

        [Authorize]
        public async Task<IActionResult> UserLogin()
        {
            var email = User.FindFirstValue("email");
            var oneLoginId = User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(oneLoginId))
            {
                _logger.LogError("Missing claims. Email: '{Email}', ID: '{Id}'", email, oneLoginId);
                TempData["ErrorMessage"] = "Unable to retrieve essential user info. Please try again.";
                return View("StartPage");
            }

            try
            {
                var existingUser = await _iUserService.GetUserByOneLoginId(oneLoginId);

                if (existingUser == null)
                {
                    var registration = new InitialUserRegistrationRequest(oneLoginId: oneLoginId, emailId: email, status: UserStatus.Active);
                    _logger.LogInformation("Submitting initial user entry. Email: {Email}, ID: {Id}", email, oneLoginId);

                    var newUserId = await _iUserService.CreateUser(registration);

                    if (string.IsNullOrWhiteSpace(newUserId))
                    {
                        _logger.LogError("API returned no valid user object.");
                        TempData["ErrorMessage"] = "Unexpected error during setup. Try again later.";
                        return View("StartPage");
                    }

                    _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserModel_Id_SessionKey, newUserId);

                    return View("StartPage");
                }

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserModel_Id_SessionKey, existingUser.Id);

                if (existingUser.Organisation != null)
                {
                    return RedirectToAction("Dashboard", "Contributor");
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during initial user registration for {Email}", email);
                TempData["ErrorMessage"] = "Error during account setup. Please contact support.";
                return View("StartPage");
            }
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // TODO - hardcoded for now, will be linked to model once we receive value from email invitaion story
            var model = new ContributorDashboardModel
            {
                OrganisationName = "ABC Org",
                HeatNetwork = "XyZ HN",
                HNStatus = "Active"
            };
            return View(model);
        }

        #endregion
    }
}
