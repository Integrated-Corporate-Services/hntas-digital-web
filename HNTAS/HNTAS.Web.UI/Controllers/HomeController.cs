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

    public HomeController(IUserService iUserService, ILogger<HomeController> logger)
    {
        _iUserService = iUserService;
        _logger = logger;
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

        try {

            // Check if user already exists in the system
            var existingUserResponse = await _iUserService.GetUserByOneLoginId(oneLoginId);
            if (existingUserResponse == null)
            {
                //Create new user entry if not found
                var registration = new InitialUserRegistrationRequest(emailId: email, oneLoginId: oneLoginId, status: UserStatus.Active);

                _logger.LogInformation("Submitting initial user entry. Email: {Email}, ID: {Id}", email, oneLoginId);

                var id = await _iUserService.CreateUser(registration);

                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogError("API returned no valid user object.");
                    TempData["ErrorMessage"] = "Unexpected error during setup. Try again later.";
                    return View();
                }

                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey, id);
            }
            else if (existingUserResponse?.Organisation == null)
            {
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey, existingUserResponse?.Id);
            }
            else
            {
                //Todo: Handle existing user case, e.g., redirect to dashboard or profile update
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey, existingUserResponse.Id);
                return RedirectToAction("UserAccount", "Dashboard");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during initial user registration for {Email}", email);
            TempData["ErrorMessage"] = "Error during account setup. Please contact support.";
            return View();
        }

        return View();
    }

    public IActionResult Error()
    {
        var errorMessage = TempData["ErrorMessage"] as string ?? "An unexpected error occurred. Please try again later.";
        return View("Error", model: errorMessage);
    }

    [HttpGet]
    public IActionResult StartPage()
    {
        return View();
    }

    [HttpGet]
    public IActionResult WhatDoYouWantToDo()
    {
        this.ShowBackButton("StartPage", "Home");
        var model = SessionHelper.GetFromSession<WhatDoYouWantToDoViewModel>(HttpContext, SessionHelper.SessionKeys.WhatDoYouWantToDoViewModelKey) ?? new WhatDoYouWantToDoViewModel();
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
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.WhatDoYouWantToDoViewModelKey, model);
                return RedirectToAction("WhereIsTheHeatNetwork", "HeatNetworkEligibility");
            case "updateExistingHN":
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.WhatDoYouWantToDoViewModelKey, model);
                return RedirectToAction("Index", "Home");
            default:
                ModelState.AddModelError(nameof(model.UserPathToday), "Invalid selection. Please try again.");
                return View();
        }
    }
}
