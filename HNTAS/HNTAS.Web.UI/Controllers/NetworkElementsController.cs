using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class NetworkElementsController : Controller
    {
        private readonly ILogger<NetworkElementsController> _logger;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IOrganisationService _organisationService;
        private readonly IAddressLookupService _addressLookUpService;

        public NetworkElementsController(ILogger<NetworkElementsController> logger, IHeatNetworkService heatNetworkService, IUserService userService, ISessionHelper sessionHelper, IOrganisationService organisationService, IAddressLookupService addressLookupService)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _organisationService = organisationService;
            _addressLookUpService = addressLookupService;
        }

        [HttpGet]
        public IActionResult SelectNetworkElements([FromRoute] Guid hnid)
        {
            this.ShowBackButton("HeatNetworkSOADetails");
            var model = new NetworkElementViewModel()
            {
                ElementOptions = Utility.GetDefaultNetworkElementOptions()
            };

            //var model = new HeatNetworkElementViewModel()
            //{
            //    ElementOptions = Utility.GetElementOptions()
            //};

            return View(model);
        }
    }
}
