using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IUserService _userService;
        private readonly IHeatNetworksApi _heatNetworksApi;
        private readonly ISessionHelper _sessionHelper;

        public DashboardController(ILogger<DashboardController> logger, IUserService userService, IHeatNetworksApi heatNetworksApi, ISessionHelper sessionHelper)
        {
            _logger = logger;
            _userService = userService;
            _heatNetworksApi = heatNetworksApi;
            _sessionHelper = sessionHelper;
        }

        public async Task<UserDetailsResponse> RetrieveUserDetails(string userId)
        {
            try
            {
                var user = await _userService.GetUserDetails(userId);

                if (user == null)
                {
                    throw new Exception("Unable to retrieve user information. Please try again later.");
                }

                if (user.Organisation == null)
                {
                    throw new Exception("Your account is not associated with any organisation. Please contact support.");
                }

                return user; // Assuming you want to return user details here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user details.");
                throw; // Rethrow the exception to be handled in the calling method            
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserAccount()
        {
            UserDetailsResponse user;
            try
            {
                user = await RetrieveUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new DashboardModel());
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationName, user.Organisation.Name);


            var dashboardModel = new DashboardModel
            {
                OrganisationName = user?.Organisation?.Name,
                UserRole = user.Roles[0].ToString(),
                IsResponsiblePerson = user.Roles?.Contains(UserRole.ResponsiblePerson) ?? false,
                HasHeatNetworks = user.HeatNetworks != null && user.HeatNetworks.Any()
            };

            return View(dashboardModel);

        }

        [HttpGet]
        public async Task<IActionResult> OrganisationDetails()
        {
            this.ShowBackButton("UserAccount");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            UserDetailsResponse user;
            bool isUserAnRP;
            try
            {
                user = await RetrieveUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
                isUserAnRP = await _userService.IsRpUserAsync(user.EmailId) ?? false;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new OrganisationDetailsModel());
            }
            _sessionHelper.SaveToSession<string>(HttpContext, "IsUserAnRP", isUserAnRP.ToString());

            var model = new OrganisationDetailsModel
            {
                OrganisationId = user.Organisation.OrgId,
                OrganisationName = user.Organisation.Name,
                RPEmail = user.EmailId,
                AddressLine1 = user.Organisation?.RegisteredAddress?.AddressLine1,
                AddressLine2 = user.Organisation?.RegisteredAddress?.AddressLine2,
                Town = user.Organisation?.RegisteredAddress?.Town,
                County = user.Organisation?.RegisteredAddress?.County,
                Postcode = user.Organisation?.RegisteredAddress?.Postcode,
                Country = user.Organisation?.RegisteredAddress?.Country
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult EditOrganisationDetails()
        {
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.IsEditOrganisationDetailsJourneySessionKey, "true");
            return RedirectToAction("OrganisationType", "Organisation");
        }
    }
}
