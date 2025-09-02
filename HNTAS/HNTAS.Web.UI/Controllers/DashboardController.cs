using HNTAS.Api.Client.Api;
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


        [HttpGet]
        public async Task<IActionResult> UserAccount()
        {
            var user = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));

            if (user == null)
            {
                _logger.LogError("User not found in session or API.");
                TempData["ErrorMessage"] = "Unable to retrieve user information. Please try again later.";
                return View(new DashboardModel());
            }

            if (user.Organisation == null)
            {
                _logger.LogError("User organisation is null.");
                TempData["ErrorMessage"] = "Your account is not associated with any organisation. Please contact support.";
                return View(new DashboardModel());
            }
            else
            {
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationName, user.Organisation.Name);
                TempData["AddressLine1"] = user.Organisation.RegisteredAddress?.AddressLine1;
                TempData["AddressLine2"] = user.Organisation.RegisteredAddress?.AddressLine2;
                TempData["Town"] = user.Organisation.RegisteredAddress?.Town;
                TempData["County"] = user.Organisation.RegisteredAddress?.County;
                TempData["Postcode"] = user.Organisation.RegisteredAddress?.Postcode;
                TempData["Country"] = user.Organisation.RegisteredAddress?.Country;
                TempData["UserEmailId"] = user.EmailId;
            }

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
                HeatNetworks = heatNetworks // This should be populated with actual data from the API
            };

            return View(dashboardModel);

        }

        [HttpGet]
        public IActionResult OrganisationDetails()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            ViewBag.AddressLine1 = TempData["AddressLine1"];
            ViewBag.AddressLine2 = TempData["AddressLine2"];
            ViewBag.Town = TempData["Town"];
            ViewBag.County = TempData["County"];
            ViewBag.Postcode = TempData["Postcode"];
            ViewBag.Country = TempData["Country"];
            ViewBag.UserEmailId = TempData["UserEmailId"];
            return View();
        }
    }
}
