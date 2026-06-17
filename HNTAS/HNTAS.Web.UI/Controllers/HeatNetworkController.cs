using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{

    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetworkDetail)]
    public class HeatNetworkController : Controller
    {

        private readonly ILogger<HeatNetworkController> _logger;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IOrganisationService _organisationService;
        private readonly IAddressLookupService _addressLookUpService;

        public HeatNetworkController(ILogger<HeatNetworkController> logger, IHeatNetworkService heatNetworkService, IUserService userService, ISessionHelper sessionHelper, IOrganisationService organisationService, IAddressLookupService addressLookupService)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _organisationService = organisationService;
            _addressLookUpService = addressLookupService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            _sessionHelper.ClearAllHNRegistrationFlowRelatedSessionData(HttpContext);
            //start
            return RedirectToAction("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
        }


        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitDetails(HNDetailsViewModel model)
        {
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, model.UHNID);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, model.Name);
            return RedirectToAction("SOAIntro", "SOA");
        }

        [HttpGet]
        public async Task<IActionResult> AddNetworkDetails([FromQuery] string hnid)
        {
            hnid = hnid ?? _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var model = await GetNetworkDetails(hnid);
            if (model == null)
                return BadRequest();

            return View("AddNetworkDetails", model);
        }

        private async Task<HNDetailsViewModel> GetNetworkDetails(string hnid)
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            // get user details
            var response = await _heatNetworkService.GetAsync(hnid?.ToUpper());

            if (response == null)
            {
                return null;
            }

            var model = new HNDetailsViewModel
            {
                Name = response?.Name,
                Address = new AddressByStreetOrTownModel
                {
                    StreetAddress = response?.Address?.AddressLine1,
                    TownOrCity = response?.Address?.Town,
                    Postalcode = response?.Address?.Postcode,
                    Country = response?.Address?.Country,
                    Fulladdress = string.Join(", ", new[] { response?.Address?.AddressLine1, response?.Address?.Town, response?.Address?.Postcode, response?.Address?.Country }.Where(part => !string.IsNullOrWhiteSpace(part)))
                },
                OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName),
                PathWay = response.Pathway,
                UHNID = response?.HnId,
                Phase = response?.Phase!
            };

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, model.UHNID);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, model.Name);

            return model;
        }

        [HttpGet]
        public async Task<IActionResult> NetworkDetails()
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);

            this.ShowBackButton("AddNetworkDetails", "HeatNetwork", new { hnId });

            var networkDetailsTypeList = NetworkElementHelper.GetDefaultNetworkDetailsOptions();            

            foreach (var option in networkDetailsTypeList)
            {
                if (option.Id == NetworkDetailsType.Soa)
                {
                    NetworkElementHelper.UpdateOptionStatus(option, heatNetworkData?.NetworkElements?.ElementSoaStatus, heatNetworkData?.NetworkElements?.NetworkElementStatus);
                }
                else if (option.Id == NetworkDetailsType.NetworkElements)
                {
                    NetworkElementHelper.UpdateOptionStatus<NetworkDetailsStatus, NetworkDetailsStatus>(option, heatNetworkData?.NetworkElements?.NetworkElementStatus, null, false);
                }                
            }

            var model = new NetworkDetailsViewModel()
            {
                DetailsOptions = networkDetailsTypeList
            };
            ViewBag.HNId = hnId;
            ViewBag.HNName = hnName;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnId);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, hnName);

            return View("NetworkDetails", model);
        }

        [HttpGet]
        public async Task<IActionResult> SelectNetworkDetail([FromQuery] string hnid, [FromQuery] NetworkDetailsType networkDetailId)
        {
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnId, hnid.ToUpper());
            ClearNetworkElementSpecificSession();
            ClearSoaAssessorSpecificSession();
            switch (networkDetailId)
            {
                case NetworkDetailsType.NetworkElements:
                    return RedirectToAction("SelectNetworkElements", "NetworkElements");
                case NetworkDetailsType.Soa:
                    return RedirectToAction("UnderstandingSoa", "ElementSoa");                
                default:
                    return BadRequest();
            }

        }

        private void ClearNetworkElementSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SelectedElementsSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SubstationViewModelKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.DistributionNetworksViewModelKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);
        }

        private void ClearSoaAssessorSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorDetailsSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorSelectedElementSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorElementSelectionOverviewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SoaStageOfAssessorOnboarding);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.DefaultSelectedAssessor);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AssessorSearchResultsSessionKey);
        }

    }
}
