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

    public HomeController(IUserService iUserService, ILogger<HomeController> logger, ISessionHelper sessionHelper)
    {
        _iUserService = iUserService;
        _logger = logger;
        _sessionHelper = sessionHelper;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var email = User.FindFirstValue("email");
        var oneLoginId = User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(oneLoginId))
        {
            _logger.LogError("Missing claims. Email: '{Email}', ID: '{Id}'", email, oneLoginId);
            TempData["ErrorMessage"] = "Unable to retrieve essential user info. Please try again.";
            return View();
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
                    return View();
                }

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserModel_Id_SessionKey, newUserId);
                return View();
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserModel_Id_SessionKey, existingUser.Id);

            if (existingUser.Organisation != null)
            {
                return RedirectToAction("UserAccount", "Dashboard");
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during initial user registration for {Email}", email);
            TempData["ErrorMessage"] = "Error during account setup. Please contact support.";
            return View();
        }
    }

    public IActionResult Error()
    {
        var errorMessage = TempData["ErrorMessage"] as string ?? "An unexpected error occurred. Please try again later.";
        return View("Error", model: errorMessage);
    }

    [HttpGet]
    public IActionResult StartPage()
    {
        _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        switch (model.UserPathToday)
        {
            case "registerNewHN":
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.WhatDoYouWantToDoViewModelKey, model);
                return RedirectToAction("WhereIsTheHeatNetwork", "HeatNetworkEligibility");
            case "updateExistingHN":
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.WhatDoYouWantToDoViewModelKey, model);
                return RedirectToAction("Index", "Home");
            default:
                ModelState.AddModelError(nameof(model.UserPathToday), "Invalid selection. Please try again.");
                return View();
        }
    }
}
