using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HNTAS.Web.UI.Controllers;
public class HomeController : Controller
{

    private readonly IUsersApi _usersApi;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IUsersApi usersApi, ILogger<HomeController> logger)
    {
        _usersApi = usersApi;
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

        var registration = new InitialUserRegistrationRequest(emailId: email, oneLoginId: oneLoginId, status: UserStatus.Active );

        try
        {
            _logger.LogInformation("Submitting initial user entry. Email: {Email}, ID: {Id}", email, oneLoginId);

            var response = await _usersApi.ApiUsersInitialEntryPostOrDefaultAsync(registration);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API call failed. Status: {Status}", response.StatusCode);
                TempData["ErrorMessage"] = "Unexpected error during setup. Try again later.";
                return View();
            }

            var apiUser = response.IsCreated ? response.Created() : response.IsOk ? response.Ok() : null;

            if (string.IsNullOrWhiteSpace(apiUser?.Id))
            {
                _logger.LogError("API returned no valid user object.");
                TempData["ErrorMessage"] = "Unexpected error during setup. Try again later.";
                return View();
            }

            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey, apiUser.Id);
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
}
