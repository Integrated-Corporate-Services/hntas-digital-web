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
                var user = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));

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
            }catch(Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new DashboardModel());
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationName, user.Organisation.Name);
                        
            ViewBag.IsRegulatoryContact = user.Roles?.Contains(Api.Client.Model.UserRole.RegulatoryContact);

            var heatNetworks = new List<HeatNetworkModel>();

            if (user.HeatNetworks != null && user.HeatNetworks?.Count > 0)
            {
                foreach (var network in user.HeatNetworks)
                {
                    heatNetworks.Add(new HeatNetworkModel
                    {
                        Name = network.Name,
                        OrganisationName = user.Organisation?.Name,
                        Status = "Active"
                    });
                }
            }

            var dashboardModel = new DashboardModel
            {
                OrganisationName = user?.Organisation?.Name,
                HeatNetworks = heatNetworks
            };

            return View(dashboardModel);

        }

        [HttpGet]
        public async Task<IActionResult> OrganisationDetails()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            UserDetailsResponse user;
            try
            {
                user = await RetrieveUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new OrganisationDetailsModel());
            }
            var model = new OrganisationDetailsModel
            {
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
    }
}
