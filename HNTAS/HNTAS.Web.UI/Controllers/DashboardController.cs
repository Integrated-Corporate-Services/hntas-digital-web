using HNTAS.Api.Client.Api;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services;
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

        public DashboardController(ILogger<DashboardController> logger, IUserService userService, IHeatNetworksApi heatNetworksApi)
        {
            _logger = logger;
            _userService = userService;
            _heatNetworksApi = heatNetworksApi;
        }

        [HttpGet]
        public async Task<IActionResult> UserAccount()
        {
            // access API to retrieve org name, all the heat networks registered
            //var organisationName = SessionHelper.GetFromSession<>

            var user = await _userService.GetUserById(SessionHelper.GetFromSession<string>(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey));
            var heatNetworksResponse = await _heatNetworksApi.ApiHeatNetworksHnIdsGetAsync(string.Join(",", user?.HnIds));

            if (user == null)
            {
                _logger.LogError("User not found in session or API.");
                TempData["ErrorMessage"] = "Unable to retrieve user information. Please try again later.";
                return View();
            }

            var heatNetworks = new List<HeatNetworkModel>();

            if (heatNetworksResponse.IsOk)
            {
              
                var heatNetworksData = heatNetworksResponse.Ok();


                foreach (var network in heatNetworksData)
                {
                    heatNetworks.Add(new HeatNetworkModel
                    {
                        Name = network.Name,
                        OrganisationName = user.Organisation?.Name,
                        Status = "Active"
                    });
                }

                var dashboardModel = new DashboardModel
                {
                    OrganisationName = user.Organisation?.Name,
                    HeatNetworks = heatNetworks // This should be populated with actual data from the API
                };

                return View(dashboardModel);
            }

           _logger.LogError("Failed to retrieve heat networks from API. Status code: {StatusCode}", heatNetworksResponse.StatusCode);
            TempData["ErrorMessage"] = "Unable to retrieve heat networks. Please try again later.";
            return View();
        }
    }
}
