using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly ISessionHelper _sessionHelper;

        public UserController(IUserService userService, ILogger<UserController> logger, ISessionHelper sessionHelper)
        {
            _logger = logger;
            _userService = userService;
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

                var user = await _userService.GetUserById(userId);

                var viewModel = new ManageUsersModel
                {
                    OrganisationName = user.Organisation?.Name,
                    Users = new List<UserDisplayModel> { new UserDisplayModel
                    {
                        Id = user.Id,
                        EmailAddress = user.EmailAddress,
                        Name = user.FullName,
                        Roles = user.Roles.Select(r => r.ToString()).ToList(),
                        Status = user.Status.ToString()
                    } }
                };

                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = Url.Action("UserAccount", "Dashboard");

                return View("ManageUsers", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to manage users.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return View("ManageUsers");
            }
        }
    }
}