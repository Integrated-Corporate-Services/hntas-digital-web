using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HNTAS.Web.UI.Controllers
{
    public class ContributorController : Controller
    {
        private readonly IUserService _iUserService;
        private readonly IInvitationService _invitationService;
        private readonly ILogger<ContributorController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IInvitationTokenService _invitationTokenService;

        public ContributorController(IUserService iUserService,
            IInvitationService invitationService,
            ILogger<ContributorController> logger,
            ISessionHelper sessionHelper,
            IInvitationTokenService invitationTokenService)
        {
            _iUserService = iUserService;
            _logger = logger;
            _sessionHelper = sessionHelper;
            _invitationTokenService = invitationTokenService;
            _invitationService = invitationService;
        }

        #region No action Pages
        [HttpGet]
        public IActionResult YouHaveDeclined()
        {
            return View();
        }

        #endregion

        #region User input Pages

        [HttpGet]
        public async Task<IActionResult> YouHaveBeenInvited()
        {
            //get token from query string
            var token = HttpContext.Request.Query["token"].ToString();
            //decrypt token to get email
            if (!string.IsNullOrWhiteSpace(token))
            {
                var (invitationId, invitationEmail) = _invitationTokenService.DecryptToken(token);
                if (invitationId == null || invitationEmail == null)
                    return BadRequest("Invalid or expired token");

                //check if invitation exists
                var invitation = await _invitationService.GetInvitationByIdAsync(invitationId);

                if (invitation == null)
                {
                    _logger.LogError("No invitation found for ID: {InvitationId}", invitationId);
                    return BadRequest("Invalid invitation details.");
                }

                var inviterUser = await _iUserService.GetUserDetails(invitation.InviterUserId);

                if (inviterUser == null || inviterUser.Organisation == null)
                {
                    _logger.LogError("Inviter user not found or has no organisation. InviterUserId: {InviterUserId}", invitation.InviterUserId);
                    return BadRequest("Invalid invitation details.");
                }

                TempData["OrgName"] = inviterUser.Organisation.Name;
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.InvitedTokenEmail, invitationEmail);
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.InvitationId, invitation.Id);
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.InvitedInviterUserId, invitation.InviterUserId);
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.InvitedInviterUserOrgId, inviterUser.Organisation.OrgId);
            }
            else
            {
                TempData["ErrorMessage"] = "The invitation token is missing from your request. Please use the link provided in the invitation email to proceed.";
            }
            var model = _sessionHelper.GetFromSession<YouHaveBeenInvitedModel>(HttpContext, SessionKeys.YouHaveBeenInvitedModelKey) ?? new YouHaveBeenInvitedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YouHaveBeenInvitedAsync(YouHaveBeenInvitedModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in invitation response.");
                return View(model);
            }

            var invitationId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.InvitationId);
            if (string.IsNullOrWhiteSpace(invitationId))
            {
                _logger.LogWarning("Invitation ID is missing from session during invitation response.");
                TempData["ErrorMessage"] = "Your session has expired or is invalid. Please use the invitation link from your email.";
                return View(model);
            }

            switch (model.AcceptInvitation?.ToLowerInvariant())
            {
                case "accept":
                    return RedirectToAction("StartPage", "Home");
                case "decline":
                    try
                    {
                        await _invitationService.RejectInvitationAsync(invitationId);
                        _logger.LogInformation("Invitation ID {InvitationId} declined.", invitationId);
                        //clear session
                        _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
                        return RedirectToAction("YouHaveDeclined", "Contributor");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while rejecting invitation ID: {InvitationId}", invitationId);
                        TempData["ErrorMessage"] = "An error occurred while declining the invitation. Please try again later.";
                        return View(model);
                    }

                default:
                    _logger.LogWarning("Invalid invitation response option: {Option}", model.AcceptInvitation);
                    ModelState.AddModelError(nameof(model.AcceptInvitation), "Please select a valid option.");
                    return View(model);
            }
        }



        [HttpGet]
        public IActionResult StartPage()
        {
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
                _logger.LogError("Missing claims. ID: '{Id}'", oneLoginId);
                TempData["ErrorMessage"] = "Unable to retrieve essential user info. Please try again.";
                return View("StartPage");
            }

            try
            {
                var existingUser = await _iUserService.GetUserByOneLoginId(oneLoginId);

                if (existingUser == null)
                {
                    var registration = new InitialUserRegistrationRequest(oneLoginId: oneLoginId, emailId: email, status: UserStatus.Active);
                    _logger.LogInformation("Submitting initial user entry. ID: {Id}", oneLoginId);

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

                if (existingUser.OrgId != null)
                {
                    return RedirectToAction("Dashboard", "Contributor");
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during initial user registration.");
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
