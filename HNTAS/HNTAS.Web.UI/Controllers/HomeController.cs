using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HNTAS.Web.UI.Controllers;
public class HomeController : Controller
{
    private readonly IUserService _iUserService;
    private readonly ILogger<HomeController> _logger;
    private readonly ISessionHelper _sessionHelper;
    private readonly IInvitationService _invitationService;

    public HomeController(IUserService iUserService,
        ILogger<HomeController> logger,
        ISessionHelper sessionHelper,
        IInvitationService invitationService)
    {
        _iUserService = iUserService;
        _logger = logger;
        _sessionHelper = sessionHelper;
        _invitationService = invitationService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var email = User.FindFirstValue("email");
        var oneLoginId = User.FindFirstValue("sub");
        var useGovUkSimulator = Environment.GetEnvironmentVariable("SIMULATOR_PROP4");

        if (!string.IsNullOrEmpty(useGovUkSimulator) && useGovUkSimulator.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            oneLoginId = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(oneLoginId))
        {
            _logger.LogError("Missing claims. Email: '{Email}', ID: '{Id}'", email, oneLoginId);
            TempData["ErrorMessage"] = "Unable to retrieve essential user info. Please try again.";
            return BadRequest();
        }

        //check for invitation flow
        var invitedEmail = User.FindFirst("hntas.invitedEmail")?.Value;
        var invitationId = User.FindFirst("hntas.invitationId")?.Value;

        if (!string.IsNullOrEmpty(invitedEmail) && !string.Equals(email, invitedEmail, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Authenticated email does not match invited email. Authenticated: '{AuthenticatedEmail}', Invited: '{InvitedEmail}'", email, invitedEmail);
            return BadRequest();
        }

        try
        {

            // If invitationId is present, we are in an invitation flow
            if (!string.IsNullOrEmpty(invitationId))
            {

                var inviterUserId = User.FindFirst("hntas.inviterUserId")?.Value;
                var inviterOrgId = User.FindFirst("hntas.inviterOrgId")?.Value;

                if (string.IsNullOrEmpty(invitedEmail) || string.IsNullOrEmpty(inviterUserId) || string.IsNullOrEmpty(inviterOrgId))
                {
                    _logger.LogError("Invitation flow data incomplete. InvitedEmail: '{InvitedEmail}' , InviterUserId: '{inviterUserId}', InviterOrgId: '{inviterOrgId}'",
                        invitedEmail, inviterUserId, inviterOrgId);

                    TempData["ErrorMessage"] = "We couldn't process your invitation due to missing information. Please try the link again or contact support if the issue persists.";
                    return BadRequest();
                }

                //check invitation is already accepted
                var invitation = await _invitationService.GetInvitationByIdAsync(invitationId);

                if (invitation == null)
                {
                    _logger.LogInformation("Invitation not found for invitationId : {invitationId}", invitationId);
                    return BadRequest();
                }

                if (invitation.Status == InvitationStatus.Invited)
                {
                    var userId = await _iUserService.AcceptUserInvitation(new InvitedUserRequest(
                        invitedEmail: invitedEmail,
                        invitationId: invitationId,
                        oneLoginId: oneLoginId));

                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        _logger.LogError("API returned no valid user object after accepting invitation.");
                        return BadRequest();
                    }

                    _logger.LogInformation($"Invitation updated successfully for the invitationId: {invitationId}, invitedEmail : {invitedEmail}");

                    _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserModel_Id_SessionKey, userId);

                    return RedirectToAction("UserAccount", "Dashboard");
                }
            }

            var existingUser = await _iUserService.GetUserByOneLoginId(oneLoginId);

            // Standard registration flow for non-invited users
            if (existingUser == null)
            {
                var registration = new InitialUserRegistrationRequest(oneLoginId: oneLoginId, emailId: email, status: UserStatus.Active);
                _logger.LogInformation("Submitting initial user entry. Email: {Email}, ID: {Id}", email, oneLoginId);

                var newUserId = await _iUserService.CreateUser(registration);

                if (string.IsNullOrWhiteSpace(newUserId))
                {
                    _logger.LogError("API returned no valid user object.");
                    TempData["ErrorMessage"] = "Unexpected error during setup. Try again later.";
                    return BadRequest();
                }

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserModel_Id_SessionKey, newUserId);

                return View();
            }
            // Existing user flow
            else if (existingUser != null)
            {
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserModel_Id_SessionKey, existingUser.Id);

                if (existingUser.OrgId == null && existingUser.Roles.Count() == 0)
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("UserAccount", "Dashboard");
                }
            }

            return RedirectToAction("UserAccount", "Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during initial user registration for {Email}", email);
            TempData["ErrorMessage"] = "Error during account setup. Please contact support.";
            return BadRequest();
        }
    }

    public IActionResult Error(int code)
    {
        if (code == 404)
            return View("NotFound");
        else if (code == 500)
            return View("Error");

        return View("Error");
    }

    [HttpGet]
    public IActionResult StartPage()
    {
        var invitedEmail = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.InvitedTokenEmail);

        if (!string.IsNullOrEmpty(invitedEmail))
        {
            this.ShowBackButton("YouHaveBeenInvited", "Contributor");
            ViewBag.NavigateUrl = Url.Action("Index", "Home");
        }
        else
        {
            ViewBag.NavigateUrl = Url.Action("WhatDoYouWantToDo");
        }

        return View();
    }

    [HttpGet]
    public IActionResult WhatDoYouWantToDo()
    {
        this.ShowBackButton("StartPage", "Home");
        var model = _sessionHelper.GetFromSession<WhatDoYouWantToDoViewModel>(HttpContext, SessionKeys.WhatDoYouWantToDoViewModelKey) ?? new WhatDoYouWantToDoViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult WhatDoYouWantToDo(WhatDoYouWantToDoViewModel model)
    {
        this.ShowBackButton("StartPage", "Home");
        if (!ModelState.IsValid)
        {
            this.ShowBackButton("StartPage", "Home");
            return View(model);
        }

        switch (model.UserPathToday)
        {
            case "registerNewHN":
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.WhatDoYouWantToDoViewModelKey, model);
                return RedirectToAction("AreYouTheRP", "HeatNetworkEligibility");
            case "updateExistingHN":
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.WhatDoYouWantToDoViewModelKey, model);
                return RedirectToAction("Index", "Home");
            default:
                ModelState.AddModelError(nameof(model.UserPathToday), "Invalid selection. Please try again.");
                return View();
        }
    }
}
