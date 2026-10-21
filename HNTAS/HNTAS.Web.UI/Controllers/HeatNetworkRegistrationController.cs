using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetwork)]
    public class HeatNetworkRegistrationController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IOrganisationService _organisationService;
        private readonly IAddressLookupService _addressLookUpService;
        private readonly IUserService _userService;
        private readonly ILogger<HeatNetworkRegistrationController> _logger;

        public HeatNetworkRegistrationController(ISessionHelper sessionHelper, IHeatNetworkService heatNetworkService, IOrganisationService organisationService, IAddressLookupService addressLookupService, IUserService userService, ILogger<HeatNetworkRegistrationController> logger)
        {
            _sessionHelper = sessionHelper;
            _heatNetworkService = heatNetworkService;
            _organisationService = organisationService;
            _addressLookUpService = addressLookupService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult HeatNetworkDwellingsCheck()
        {            
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("UserAccount", "Dashboard");

            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedModel();
            return View("HeatNetworkRegistration/HeatNetworkDwellingsCheck", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkDwellingsCheck(HowManyDwellingsIncludedModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("UserAccount", "Dashboard");
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkDwellingsCheck", model);
            }
            _sessionHelper.SaveToSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
            switch (model.HowManyDwellingsIncluded)
            {
                case "yes":
                    return RedirectToAction("HeatNetworkStatus");
                case "no":
                default:
                    return RedirectToAction("SixOrMoreDwellingsAnswerNo");
            }
        }

        [HttpGet]
        public IActionResult SixOrMoreDwellingsAnswerNo()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
            return View("HeatNetworkRegistration/SixOrMoreDwellingsAnswerNo");
        }

        [HttpGet]
        public IActionResult HeatNetworkStatus()
        {
            this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
            var sessionModel = _sessionHelper.GetFromSession<NetworkStatusModel>(HttpContext, SessionKeys.NetworkStatusModelKey);
            var model = sessionModel ?? new NetworkStatusModel();
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);

            return View("HeatNetworkRegistration/HeatNetworkStatus", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHeatNetworkStatus(NetworkStatusModel model)
        {

            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
           
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
                return View("HeatNetworkRegistration/HeatNetworkStatus", model);
            }

            _sessionHelper.SaveToSession<NetworkStatusModel>(HttpContext, SessionKeys.NetworkStatusModelKey, model);
            
            if (string.Equals(model.BuildStatus, NullableOfNetworkBuildStatus.New.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("HeatNetworkEcNewNetwork");
            }

            if (string.Equals(model.BuildStatus, NullableOfNetworkBuildStatus.Existing.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("ExistingHeatNetworkOfgemSummary", "HeatNetworkRegistration");
            }

            // Handles "BOTH" or any unhandled value
            return RedirectToAction("HeatNetworkEcNewNetwork");
        }

        [HttpGet]
        public IActionResult ExistingHeatNetworkOfgemSummary()
        {
            this.ShowBackButton("HeatNetworkStatus", "HeatNetworkRegistration");
            return View();
        }

        public IActionResult HeatNetworkEcNewNetwork()
        {
            this.ShowBackButton("HeatNetworkStatus", "HeatNetworkRegistration");

            var model = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(
                HttpContext,
                SessionKeys.NetworkSupplyTypeModelKey
            ) ?? new NetworkSupplyTypeModel();

            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            return View("HeatNetworkEcNewNetwork", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveECNewNetwork(NetworkSupplyTypeModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("HeatNetworkStatus", "HeatNetworkRegistration");
                return View("HeatNetworkEcNewNetwork", model);
            }

            var energyCentreOption = model?.EnergyCentreOption;

            // Evaluate flags
            bool isCommunalHn = energyCentreOption == NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString() ||
                                energyCentreOption == NullableOfEnergyCentreType.SingleCommunalBuildingSeparateDistrictNetwork.ToString();

            bool hasOwnEc = energyCentreOption == NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString() ||
                            energyCentreOption == NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString();

            // Persist flags and model to session early
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.IsCommualNetworkKey, isCommunalHn);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HasOwnEnergyCentreKey, hasOwnEc);

            var existingNetworkSupplyTypeModel = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(
                HttpContext,
                SessionKeys.NetworkSupplyTypeModelKey
            );

            // If the selection changed, reset dependent connection/responsibility models
            if (existingNetworkSupplyTypeModel?.EnergyCentreOption != energyCentreOption)
            {
                _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkConnectionsModelKey);
                _sessionHelper.ClearFromSession(HttpContext, SessionKeys.CommunalBuildingResponsibilityModelKey);
                _sessionHelper.ClearFromSession(HttpContext, SessionKeys.DistrictNetworksResponsibilityModelKey);
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkSupplyTypeModelKey, model);

            // Check if this option matches either scenario requiring a preset connection summary flow
            bool isSingleCommunalOnly = energyCentreOption == NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString();
            bool isSingleCommunalWithSeparateDistrict = energyCentreOption == NullableOfEnergyCentreType.SingleCommunalBuildingSeparateDistrictNetwork.ToString();

            if (isSingleCommunalOnly || isSingleCommunalWithSeparateDistrict)
            {
                var buildStatusModel = _sessionHelper.GetFromSession<NetworkStatusModel>(
                    HttpContext,
                    SessionKeys.NetworkStatusModelKey
                );

                var buildStatus = buildStatusModel?.BuildStatus;

                var connection = new NetworkConnectionsViewModel
                {
                    NewCommunalBuildings = 1,
                    NewOtherDistrictNetworks = isSingleCommunalWithSeparateDistrict ? 1 : (int?)null,
                    ShowExistingColumn = buildStatus == NullableOfNetworkBuildStatus.Existing.ToString() ||
                                         buildStatus == NullableOfNetworkBuildStatus.NewAndExisting.ToString(),
                    ShowNewColumn = buildStatus == NullableOfNetworkBuildStatus.New.ToString() ||
                                    buildStatus == NullableOfNetworkBuildStatus.NewAndExisting.ToString(),
                    ShowOtherDistrictNetworksRow = string.Equals(energyCentreOption,
                                                                 NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString(),
                                                                 StringComparison.OrdinalIgnoreCase)
                };

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkConnectionsModelKey, connection);

                return RedirectToAction("HeatNetworkConnectionSummary");
            }

            return RedirectToAction("HeatNetworkConnections");
        }


        [HttpGet]
        public IActionResult HeatNetworkConnections()
        {
            // Configure back button navigation for GDS standards
            this.ShowBackButton("HeatNetworkEcNewNetwork", "HeatNetworkRegistration");

            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            // Retrieve previous steps from session
            var statusModel = _sessionHelper.GetFromSession<NetworkStatusModel>(
                HttpContext,
                SessionKeys.NetworkStatusModelKey
            );

            var supplyTypeModel = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(
                HttpContext,
                SessionKeys.NetworkSupplyTypeModelKey
            );

            // Retrieve saved connections if user is navigating back
            var existingConnectionsModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(
                HttpContext,
                SessionKeys.NetworkConnectionsModelKey
            );

            var viewModel = existingConnectionsModel ?? new NetworkConnectionsViewModel();

            // Determine column visibility flags based on build status
            var buildStatus = statusModel?.BuildStatus;
            viewModel.ShowExistingColumn = buildStatus == NullableOfNetworkBuildStatus.Existing.ToString() || buildStatus == NullableOfNetworkBuildStatus.NewAndExisting.ToString();
            viewModel.ShowNewColumn = buildStatus == NullableOfNetworkBuildStatus.New.ToString() || buildStatus == NullableOfNetworkBuildStatus.NewAndExisting.ToString();

            // Determine row visibility: Show "Other district heat networks" row only when supplying multiple buildings
            var energyCentreOption = supplyTypeModel?.EnergyCentreOption;
            viewModel.ShowOtherDistrictNetworksRow = string.Equals(energyCentreOption, NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString(), StringComparison.OrdinalIgnoreCase);

            return View("TellUsAboutHeatNetworkConnections", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHeatNetworkConnections(NetworkConnectionsViewModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            // 1. Clear validation errors for inactive columns based on context flags
            if (!model.ShowExistingColumn)
            {
                ModelState.Remove(nameof(model.ExistingCommunalBuildings));
                ModelState.Remove(nameof(model.ExistingHousesOrMaisonettes));
                ModelState.Remove(nameof(model.ExistingNonDomesticBuildings));
                ModelState.Remove(nameof(model.ExistingMultiTenantedNonDomestic));
                ModelState.Remove(nameof(model.ExistingOtherDistrictNetworks));
            }

            if (!model.ShowNewColumn)
            {
                ModelState.Remove(nameof(model.NewCommunalBuildings));
                ModelState.Remove(nameof(model.NewHousesOrMaisonettes));
                ModelState.Remove(nameof(model.NewNonDomesticBuildings));
                ModelState.Remove(nameof(model.NewMultiTenantedNonDomestic));
                ModelState.Remove(nameof(model.NewOtherDistrictNetworks));
            }

            // 2. Clear validation errors for 'Other District Networks' row if hidden
            if (!model.ShowOtherDistrictNetworksRow)
            {
                ModelState.Remove(nameof(model.ExistingOtherDistrictNetworks));
                ModelState.Remove(nameof(model.NewOtherDistrictNetworks));
            }

            var statusModel = _sessionHelper.GetFromSession<NetworkStatusModel>(HttpContext, SessionKeys.NetworkStatusModelKey);

            var supplyTypeModel = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(HttpContext,SessionKeys.NetworkSupplyTypeModelKey);

            if (statusModel?.BuildStatus == NullableOfNetworkBuildStatus.New.ToString())
            {
                if (supplyTypeModel?.EnergyCentreOption == NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString()
                    || supplyTypeModel?.EnergyCentreOption == NullableOfEnergyCentreType.SeparateDistrictNetworkMultipleBuildings.ToString())
                {
                    // Calculate total sum of connections across all fields (handling potential nulls safely)
                    var sumOfConnections = (model.NewCommunalBuildings ?? 0) +
                                           (model.NewHousesOrMaisonettes ?? 0) +
                                           (model.NewNonDomesticBuildings ?? 0) +
                                           (model.NewMultiTenantedNonDomestic ?? 0) +
                                           (model.NewOtherDistrictNetworks ?? 0);

                    // Rule 2.a: Total connections less than 2
                    // Rule 2.b: Exactly 1 communal building and all other connections are 0 (which also results in sum = 1 < 2)
                    if (sumOfConnections < 2)
                    {
                        ModelState.AddModelError(nameof(model.NewCommunalBuildings), "You indicated multiple buildings. Enter a minimum of 2 connections.");
                    }
                }

                if(supplyTypeModel?.EnergyCentreOption == NullableOfEnergyCentreType.SeparateDistrictNetworkMultipleBuildings.ToString())
                {
                    //default to NewOtherDistrictNetworks to 1
                    model.NewOtherDistrictNetworks = 1;
                }
            }

            // 3. Handle model validation failure
            if (!ModelState.IsValid)
            {
                TempData["ErrorSummary"] = "CustomErrorSummary";
                this.ShowBackButton("HeatNetworkEcNewNetwork", "HeatNetworkRegistration");
                return View("TellUsAboutHeatNetworkConnections", model);
            }

            // 4. Save connections data to session
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkConnectionsModelKey, model);

            // 5. Check new communal buildings count for conditional routing
            int newCommunalBuildings = model.NewCommunalBuildings ?? 0;
            int newOtherDistricBuildings = model.NewOtherDistrictNetworks ?? 0;

            if (newCommunalBuildings > 0)
            {
                return RedirectToAction("CommunalBuildingResponsibility");
            }
            else if(newOtherDistricBuildings > 0)
            {
                return RedirectToAction("DistrictNetworksResponsibility");
            }

            return RedirectToAction("HeatNetworkConnectionSummary");
        }


        [HttpGet]
        public IActionResult CommunalBuildingResponsibility()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");

            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            var connectionsModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(
                HttpContext,
                SessionKeys.NetworkConnectionsModelKey
            );

            var existingModel = _sessionHelper.GetFromSession<CommunalBuildingResponsibilityViewModel>(
                HttpContext,
                SessionKeys.CommunalBuildingResponsibilityModelKey
            );

            // If first visit, create new model (defaults ResponsibleCommunalNetworks to 1)
            var viewModel = existingModel ?? new CommunalBuildingResponsibilityViewModel();

            viewModel.MaxAllowedCommunalBuildings = connectionsModel?.NewCommunalBuildings ?? 0;

            return View("CommunalBuildingResponsibility", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCommunalBuildingResponsibility(CommunalBuildingResponsibilityViewModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            var connectionsModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(
                HttpContext,
                SessionKeys.NetworkConnectionsModelKey
            );

            int maxAllowed = connectionsModel?.NewCommunalBuildings ?? 0;
            model.MaxAllowedCommunalBuildings = maxAllowed;

            // Validate that the value entered does not exceed total new communal buildings
            if (model.ResponsibleCommunalNetworks.HasValue && model.ResponsibleCommunalNetworks.Value > maxAllowed)
            {
                ModelState.AddModelError(
                    nameof(model.ResponsibleCommunalNetworks),
                    $"The communal building input number cannot be greater than the numbered entered on the connection screen."
                );
            }

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
                return View("CommunalBuildingResponsibility", model);
            }

            // Save to session (saves 0, 1, or whatever valid number the user posted)
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.CommunalBuildingResponsibilityModelKey, model);

            // Skip this step if there are no new district networks to report
            if (connectionsModel == null || (connectionsModel.NewOtherDistrictNetworks ?? 0) < 1)
            {
                return RedirectToAction("HeatNetworkConnectionSummary");
            }

            return RedirectToAction("DistrictNetworksResponsibility");
        }


        [HttpGet]
        public IActionResult DistrictNetworksResponsibility()
        {
            var connectionsModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(
                HttpContext,
                SessionKeys.NetworkConnectionsModelKey
            );

            if(connectionsModel?.NewCommunalBuildings > 0)
            {
                this.ShowBackButton("CommunalBuildingResponsibility", "HeatNetworkRegistration");
            }
            else
            {
                this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            }

            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            var existingModel = _sessionHelper.GetFromSession<DistrictNetworksResponsibilityViewModel>(
                HttpContext,
                SessionKeys.DistrictNetworksResponsibilityModelKey
            );

            var viewModel = existingModel ?? new DistrictNetworksResponsibilityViewModel();

            viewModel.MaxAllowedDistrictNetworks = connectionsModel.NewOtherDistrictNetworks ?? 0;

            return View("DistrictNetworksResponsibility", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DistrictNetworksResponsibility(DistrictNetworksResponsibilityViewModel model)
        {
            var connectionsModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(
                HttpContext,
                SessionKeys.NetworkConnectionsModelKey
            );

            int maxAllowed = connectionsModel?.NewOtherDistrictNetworks ?? 0;
            model.MaxAllowedDistrictNetworks = maxAllowed;

            if (model.ResponsibleDistrictNetworks.HasValue && model.ResponsibleDistrictNetworks.Value > maxAllowed)
            {
                ModelState.AddModelError(
                    nameof(model.ResponsibleDistrictNetworks),
                    $"The connected district networks input number cannot be greater than the numbered entered on the connection screen."
                );
            }

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
                return View("DistrictNetworksResponsibility", model);
            }

            // Save to session and navigate to next step
            _sessionHelper.SaveToSession(
                HttpContext,
                SessionKeys.DistrictNetworksResponsibilityModelKey,
                model
            );

            return RedirectToAction("HeatNetworkConnectionSummary");
        }

        [HttpGet]
        public IActionResult HeatNetworkConnectionSummary()
        {
            var connectionsModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(
                HttpContext,
                SessionKeys.NetworkConnectionsModelKey
            );

            var energyCentreModel = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(
                HttpContext,
                SessionKeys.NetworkSupplyTypeModelKey
            );

            ViewBag.ChangeAction = "HeatNetworkConnections";

            var energyCentreOption = energyCentreModel?.EnergyCentreOption ?? string.Empty;

            // Dynamic back button depending on whether communal building responsibility was required
            if (energyCentreOption == NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString()
                || energyCentreOption == NullableOfEnergyCentreType.SingleCommunalBuildingSeparateDistrictNetwork.ToString())
            {
                this.ShowBackButton("HeatNetworkEcNewNetwork", "HeatNetworkRegistration");
                ViewBag.ChangeAction = "HeatNetworkEcNewNetwork";
            }
            else if((connectionsModel?.NewCommunalBuildings ?? 0) > 0)
            {
                this.ShowBackButton("CommunalBuildingResponsibility", "HeatNetworkRegistration");
            }
            else if ((connectionsModel?.NewOtherDistrictNetworks ?? 0) > 0)
            {
                this.ShowBackButton("DistrictNetworksResponsibility", "HeatNetworkRegistration");
            }
            else
            {
                bool isSingleCommunalOnly = energyCentreOption == NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString();
                bool isSingleCommunalWithSeparateDistrict = energyCentreOption == NullableOfEnergyCentreType.SingleCommunalBuildingSeparateDistrictNetwork.ToString();

                if (isSingleCommunalOnly || isSingleCommunalWithSeparateDistrict)
                {
                    this.ShowBackButton("HeatNetworkEcNewNetwork", "HeatNetworkRegistration");
                }
                else
                {
                    this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
                }
            }


            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            var responsibilityModel = _sessionHelper.GetFromSession<CommunalBuildingResponsibilityViewModel>(
                HttpContext,
                SessionKeys.CommunalBuildingResponsibilityModelKey
            );

            var viewModel = new HeatNetworkConnectionSummaryViewModel
            {
                Connections = connectionsModel ?? new NetworkConnectionsViewModel(),
                EnergyCentreOption = energyCentreModel?.EnergyCentreOption ?? string.Empty,
                Responsibility = responsibilityModel
            };

            return View("HeatNetworkConnectionSummary", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHeatNetworkConnectionSummary()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController)
                .Replace("Controller", string.Empty);

            // Save summary confirmation state if required and redirect to next registration step
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkConnectionSummary");
            return RedirectToAction("HeatNetworkName");
        }

        [HttpGet]
        public IActionResult HeatNetworkName()
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backActionFromHnName");
            this.ShowBackButton(backAction);
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();

            return View("HeatNetworkRegistration/HeatNetworkName", heatNetworkNameModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkName(HeatNetworkNameModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backActionFromHnName");
            this.ShowBackButton(backAction);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkName", model);
            }
            _sessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey, model);

            return RedirectToAction("DoesHNHaveAPostcode");
        }

        #region address input
        [HttpGet]
        public IActionResult DoesHNHaveAPostcode()
        {            
            this.ShowBackButton("HeatNetworkName");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);

            NetworkSupplyTypeModel networkSupplyTypeModel = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(HttpContext, SessionKeys.NetworkSupplyTypeModelKey);

            var addressFor =
                networkSupplyTypeModel?.EnergyCentreOption == NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString() ? "main energy centre" :
                networkSupplyTypeModel?.EnergyCentreOption == NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString() ? "energy centre" : "heat network";
                
            _sessionHelper.SaveToSession<string>(HttpContext, "addressFor", addressFor);
            ViewBag.addressFor = addressFor;
            var model = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey) ?? new DoesHNHaveAPostcodeViewModel();
            return View("HeatNetworkRegistration/DoesHNHaveAPostcode", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoesHNHaveAPostcode(DoesHNHaveAPostcodeViewModel model)
        {
            this.ShowBackButton("HeatNetworkName");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");

            if (!ModelState.IsValid)
            {                
                return View("HeatNetworkRegistration/DoesHNHaveAPostcode", model);
            }            
            if ((bool)!model.HasPostcode!)
            {
                model.Postcode = null;
                _sessionHelper.SaveToSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey, model);
                _sessionHelper.SaveToSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey, null);
                _sessionHelper.SaveToSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey, null);
                _sessionHelper.SaveToSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey, null);
                return RedirectToAction("ECCoordinates");
            }
            else
            {
                SearchAddressByPostcodeModel results = await _addressLookUpService.PostcodeLookupAsync(model.Postcode);
                model.Postcode = model.Postcode?.ToUpperInvariant().Trim();
                if (results == null || results.Addresses == null || results.Addresses.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode. Please try again later");
                    return View("HeatNetworkRegistration/DoesHNHaveAPostcode", model);
                }
                results.Addresses = results.Addresses
                    .Select(address => Utility.CapitalizeCommaSeparated(address))
                    .ToArray();
                _sessionHelper.SaveToSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey, model);
                _sessionHelper.SaveToSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey, results);

                return RedirectToAction("SearchByPostcodeResults");
            }            
        }

        [HttpGet]
        public IActionResult SearchByPostcodeResults()
        {
            this.ShowBackButton("DoesHNHaveAPostcode");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            SearchAddressByPostcodeModel model = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);

            if (model == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel is null.");
                return View("HeatNetworkRegistration/DoesHNHaveAPostcode");
            }
            return View("HeatNetworkRegistration/SearchByPostcodeResults", model);
        }

        [HttpGet]
        public IActionResult SelectAddress(string selectedAddress)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var addressmodel = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);

            if (addressmodel == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel not found in session.");
                return BadRequest("Session expired or invalid. Please try again.");
            }
            addressmodel.SelectedFullAddress = Utility.CapitalizeCommaSeparated(selectedAddress);
            var addressParts = addressmodel.SelectedFullAddress.Split(",");
            if (addressParts.Length < 3)
            {
                var sanitizedAddress = selectedAddress?
                    .Replace("\r", " ")
                    .Replace("\n", " ");
                _logger.LogError("Malformed address received: {Address}", sanitizedAddress);
                return BadRequest("Selected address is not in the expected format. It must contain at least street, town/city, and postcode.");
            }

            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = string.Join(",", addressParts.Take(addressParts.Length - 2)).Trim() ?? string.Empty,
                TownOrCity = addressParts[addressParts.Length - 2].Trim() ?? string.Empty,
                Postalcode = (addressParts[addressParts.Length - 1]).ToUpper().Trim() ?? string.Empty,
                Country = "United Kingdom" ?? string.Empty,
                Fulladdress = addressmodel.SelectedFullAddress
            };
            // Save the new model to session
            _sessionHelper.SaveToSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey, model);
            return RedirectToAction("ConfirmAddress");
        }

        [HttpGet]
        public IActionResult AddressManualEntry()
        {
            this.ShowBackButton("HeatNetworkName", "HeatNetworkRegistration");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            var model = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey)?.HNAddressByStreet ?? new AddressByStreetOrTownModel { Country = "United Kingdom" };
            return View("HeatNetworkRegistration/AddressManualEntry", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddressManualEntry(AddressByStreetOrTownModel model)
        {
            this.ShowBackButton("HeatNetworkName");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");

            if (string.IsNullOrEmpty(model.Postalcode))
            {
                ModelState.AddModelError(nameof(model.Postalcode), "Enter the postcode");
            }

            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/AddressManualEntry", model);
            }
            var addressParts = new[] { model.StreetAddress, model.TownOrCity, model.Postalcode.ToUpper(), model.Country }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            model.Fulladdress = string.Join(", ", addressParts);
            var heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            heatNetworkLocationModel.HNAddressByStreet = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);

            return RedirectToAction("ECCoordinates");
        }

        [HttpGet]
        public IActionResult ConfirmAddress()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey);
            return View("HeatNetworkRegistration/ConfirmAddress", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAddress(AddressByStreetOrTownModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return RedirectToAction("SaveHNAddressByPostcode");
        }
        #endregion

        [HttpGet]
        public IActionResult SaveHNAddressByPostcode()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey) ?? new AddressByStreetOrTownModel();            
            var doesHnHaveAPostcodeModel = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel;
            if (doesHnHaveAPostcodeModel?.HasPostcode == true)
            {
                heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel { HNAddressByStreet = new AddressByStreetOrTownModel() };
                heatNetworkLocationModel.HNAddressByStreet = model;
            }
            else
            {
                heatNetworkLocationModel = null;
            }           
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);
            return RedirectToAction("ECCoordinates");
        }

        #region Coordinates input
        [HttpGet]
        public IActionResult ECCoordinates()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);

            var isCommunalHn = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.IsCommualNetworkKey);
            var hasOwnEc = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.HasOwnEnergyCentreKey);

            this.ShowBackButton("DoesHNHaveAPostcode");
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            //Todo: this need fixing when we do the existing journey, for now we will just use the same question for both with and without EC
            ViewBag.QuestionForWithEC = $"What are the {ViewBag.addressFor}'s grid coordinates?";
            ViewBag.QuestionForWithoutEC = $"What are the {ViewBag.addressFor}'s grid coordinates?";
            ViewBag.WithEc = hasOwnEc;
            var model = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey) ?? new ECDetailsModel { ECAddressByLatLong = new AddressByLatLongModel() };
            return View("HeatNetworkRegistration/ECCoordinates", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ECCoordinates(ECDetailsModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var isCommunalHn = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel)?.IsHnTypeCommunal ?? false;
            bool hasOwnEc = isCommunalHn
                ? (_sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel)?.HasOwnEc == true)
                : (_sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel)?.HasOwnEc == true);         
            this.ShowBackButton(!isCommunalHn && !hasOwnEc ? "HeatNetworkName" : "DoesHNHaveAPostcode");
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            ViewBag.QuestionForWithEC = $"What are the {ViewBag.addressFor}'s grid coordinates?";
            ViewBag.QuestionForWithoutEC = $"What are the {ViewBag.addressFor}'s grid coordinates?";
            ViewBag.WithEc = hasOwnEc;
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/ECCoordinates", model);
            }

            // Try to split and parse the LatitudeLongitude value
            var raw = model.LatitudeLongitude;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var parts = raw.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

                if (parts.Length != 2
                    || !decimal.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lat)
                    || !decimal.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lon))
                {
                    ModelState.AddModelError(nameof(model.LatitudeLongitude),
                        "Enter the latitude and longitude in the correct format");
                    return View("HeatNetworkRegistration/ECCoordinates", model);
                }

                // Populate nested AddressByLatLongModel
                model.ECAddressByLatLong.Latitude = lat;
                model.ECAddressByLatLong.Longitude = lon;
            }
           
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ECDetailsModelSessionKey, model);
            return RedirectToAction("HeatNetworkPhase");
        }
        #endregion

        

        [HttpGet]
        public IActionResult HeatNetworkPhase()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("ECCoordinates");
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey) ?? new HeatNetworkPhaseModel();            
            return View("HeatNetworkRegistration/HeatNetworkPhase", heatNetworkPhaseModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkPhase(HeatNetworkPhaseModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("ECCoordinates");
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkPhase", model);
            }
            else if (string.IsNullOrWhiteSpace(model.HeatNetworkPhase))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase");
                return View("HeatNetworkRegistration/HeatNetworkPhase", model);
            }
            else
            {
                _sessionHelper.SaveToSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model);
                switch (model.HeatNetworkPhase)
                {
                    case "Feasibility":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    case "Design":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "2" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");                    
                    case "Construction":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "3" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    case "Operation":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "4" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    default:
                        ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase");
                        return View("HeatNetworkRegistration/HeatNetworkPhase", model);
                }
            }
        }

        [HttpGet]
        public IActionResult HeatNetworkLeaveService()
        {
            this.ShowBackButton("HeatNetworkPhase");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return View("HeatNetworkRegistration/HeatNetworkLeaveService");
        }

        [HttpGet]
        public async Task<IActionResult> CheckYourAnswersAsync()
        {            
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.ShowBackButton = false;

            HowManyDwellingsIncludedModel howManyDwellingsIncludedModel = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey);
            NetworkStatusModel networkStatusModel = _sessionHelper.GetFromSession<NetworkStatusModel>(HttpContext, SessionKeys.NetworkStatusModelKey);
            NetworkSupplyTypeModel networkSupplyTypeModel = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(HttpContext, SessionKeys.NetworkSupplyTypeModelKey);
            NetworkConnectionsViewModel networkConnectionsViewModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(HttpContext, SessionKeys.NetworkConnectionsModelKey);
            CommunalBuildingResponsibilityViewModel communalBuildingResponsibilityViewModel = _sessionHelper.GetFromSession<CommunalBuildingResponsibilityViewModel>(HttpContext, SessionKeys.CommunalBuildingResponsibilityModelKey);
            DistrictNetworksResponsibilityViewModel districtNetworksResponsibilityViewModel = _sessionHelper.GetFromSession<DistrictNetworksResponsibilityViewModel>(HttpContext, SessionKeys.DistrictNetworksResponsibilityModelKey);
            HeatNetworkNameModel heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            ECDetailsModel ecDetailsModel = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey);
            HeatNetworkPhaseModel heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey);

            var isCommunalHn = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.IsCommualNetworkKey);
            var hasOwnEc = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.HasOwnEnergyCentreKey);


            if (heatNetworkNameModel == null || heatNetworkPhaseModel == null || networkSupplyTypeModel == null || networkStatusModel == null || networkConnectionsViewModel == null )
            {
                return RedirectToAction("UserAccount", "Dashboard");
            }

            var model = new CheckYourAnswersHeatNetworkModel
            {
                DoesHnHaveMoreThan6Dwellings = howManyDwellingsIncludedModel.HowManyDwellingsIncluded == "yes" ? "Yes" : "No",
                HeatNetworkType = isCommunalHn ? "Communal" : "District",
                NetworkStatusModel = networkStatusModel,
                NetworkConnectionsModel = networkConnectionsViewModel,
                ECDetailsModel = ecDetailsModel,
                HeatNetworkNameModel = heatNetworkNameModel,
                HeatNetworkAddressModel = heatNetworkLocationModel?.HNAddressByStreet ?? null,
                HeatNetworkPhaseModel = heatNetworkPhaseModel,
                PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey) ?? new PathwayModel() { Pathway = "1" },
                ConfirmedDeclaration = false
            };

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;

            _sessionHelper.SaveToSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey, model);

            ViewBag.AddressPostCodeLable = 
               networkSupplyTypeModel?.EnergyCentreOption == NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString() ? "Main energy centre" :
               networkSupplyTypeModel?.EnergyCentreOption == NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString() ? "Energy centre" : "Heat network";
            ViewBag.AddressPostCodeLable += " address and postcode";

            return View("HeatNetworkRegistration/CheckYourAnswers", model);
        }

        // Check what to add in db for type and connections


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(bool ConfirmedDeclaration)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var viewModel = _sessionHelper.GetFromSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey);

            HowManyDwellingsIncludedModel howManyDwellingsIncludedModel = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey);
            NetworkStatusModel networkStatusModel = _sessionHelper.GetFromSession<NetworkStatusModel>(HttpContext, SessionKeys.NetworkStatusModelKey);
            NetworkSupplyTypeModel networkSupplyTypeModel = _sessionHelper.GetFromSession<NetworkSupplyTypeModel>(HttpContext, SessionKeys.NetworkSupplyTypeModelKey);
            NetworkConnectionsViewModel networkConnectionsViewModel = _sessionHelper.GetFromSession<NetworkConnectionsViewModel>(HttpContext, SessionKeys.NetworkConnectionsModelKey);
            CommunalBuildingResponsibilityViewModel communalBuildingResponsibilityViewModel = _sessionHelper.GetFromSession<CommunalBuildingResponsibilityViewModel>(HttpContext, SessionKeys.CommunalBuildingResponsibilityModelKey);
            DistrictNetworksResponsibilityViewModel districtNetworksResponsibilityViewModel = _sessionHelper.GetFromSession<DistrictNetworksResponsibilityViewModel>(HttpContext, SessionKeys.DistrictNetworksResponsibilityModelKey);
            HeatNetworkNameModel heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            ECDetailsModel ecDetailsModel = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey);
            HeatNetworkPhaseModel heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey);
            DoesHNHaveAPostcodeViewModel doesHNHaveAPostcodeViewModel = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey);

            ModelState.Clear();

            // Validate the mandatory checkbox
            if (ConfirmedDeclaration != true)
            {
                ModelState.AddModelError(nameof(viewModel.ConfirmedDeclaration), "Confirm that the summary is correct");
            }

            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/CheckYourAnswers", viewModel);
            }

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            if (userId == null || orgId == null)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later";
                return View("HeatNetworkRegistration/CheckYourAnswers", viewModel);
            }

            var isCommunalHn = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.IsCommualNetworkKey);
            var hasOwnEc = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.HasOwnEnergyCentreKey);

            HeatNetworkType hnType = isCommunalHn switch
            {
                true => HNTAS.Api.Client.Model.HeatNetworkType.Communal,
                false => HNTAS.Api.Client.Model.HeatNetworkType.District
            };

            HeatNetworkConnections heatNetworkConnections = new HeatNetworkConnections();

            if (networkConnectionsViewModel == null)
            {
                TempData["ErrorMessage"] = "Your session has expired. Please start again.";
                return RedirectToAction("HeatNetworkConnections");
            }

            if (networkConnectionsViewModel != null && networkStatusModel?.BuildStatus == NullableOfNetworkBuildStatus.NewAndExisting.ToString())
            {
                heatNetworkConnections.NoOfExistingCommunalBuilding = networkConnectionsViewModel.ExistingCommunalBuildings;
                heatNetworkConnections.NoOfExistingHousesOrMaisonettes = networkConnectionsViewModel.ExistingHousesOrMaisonettes;
                heatNetworkConnections.NoOfExistingMultiTenantedNonDomestic = networkConnectionsViewModel.ExistingMultiTenantedNonDomestic;
                heatNetworkConnections.NoOfExistingNonDomesticBuildings = networkConnectionsViewModel.ExistingNonDomesticBuildings;
                heatNetworkConnections.NoOfExistingOtherDistrictNetworks = networkConnectionsViewModel.ExistingOtherDistrictNetworks;

                heatNetworkConnections.NoOfNewCommunalBuilding = networkConnectionsViewModel.NewCommunalBuildings;
                heatNetworkConnections.NoOfNewHousesOrMaisonettes = networkConnectionsViewModel.NewHousesOrMaisonettes;
                heatNetworkConnections.NoOfNewMultiTenantedNonDomestic = networkConnectionsViewModel.NewMultiTenantedNonDomestic;
                heatNetworkConnections.NoOfNewNonDomesticBuildings = networkConnectionsViewModel.NewNonDomesticBuildings;
                heatNetworkConnections.NoOfNewOtherDistrictNetworks = networkConnectionsViewModel.NewOtherDistrictNetworks;
            }

            if (networkConnectionsViewModel != null && networkStatusModel?.BuildStatus == NullableOfNetworkBuildStatus.New.ToString())
            {
                heatNetworkConnections.NoOfNewCommunalBuilding = networkConnectionsViewModel.NewCommunalBuildings;
                heatNetworkConnections.NoOfNewHousesOrMaisonettes = networkConnectionsViewModel.NewHousesOrMaisonettes;
                heatNetworkConnections.NoOfNewMultiTenantedNonDomestic = networkConnectionsViewModel.NewMultiTenantedNonDomestic;
                heatNetworkConnections.NoOfNewNonDomesticBuildings = networkConnectionsViewModel.NewNonDomesticBuildings;
                heatNetworkConnections.NoOfNewOtherDistrictNetworks = networkConnectionsViewModel.NewOtherDistrictNetworks;
            };

            var hnAddress = viewModel?.HeatNetworkAddressModel;

            double? latitude = null;
            double? longitude = null;
            if (viewModel?.ECDetailsModel?.ECAddressByLatLong != null)
            {
                latitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Latitude;
                longitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Longitude;
            }

            ECDetails? ecDetails = (latitude.HasValue || longitude.HasValue)
                ? new ECDetails(latitude: latitude, longitude: longitude)
                : null;

            var address = viewModel.HeatNetworkAddressModel != null ? new RegisteredAddress(
                    addressLine1: hnAddress?.StreetAddress?.Trim(),
                    postcode: hnAddress?.Postalcode?.Trim(),
                    addressLine2: default,
                    town: hnAddress?.TownOrCity?.Trim(),
                    county: default,
                    country: hnAddress?.Country?.Trim()
                ) : null;
            
            ViewBag.RegistrationSource = RegistrationSource.HNTAS;

            var model = new HeatNetwork
            {
                OrgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId),
                Name = viewModel?.HeatNetworkNameModel?.HeatNetworkName,
                AdditionalDescription = viewModel?.HeatNetworkNameModel?.AdditionalDescription,
                SuppliesSixOrMoreUnits = true, // cannot create heat network, unless true
                NetworkBuildStatus = (NullableOfNetworkBuildStatus)Enum.Parse(typeof(NullableOfNetworkBuildStatus), networkStatusModel?.BuildStatus),
                EnergyCentreType = (NullableOfEnergyCentreType)Enum.Parse(typeof(NullableOfEnergyCentreType), networkSupplyTypeModel?.EnergyCentreOption),
                HasAddressAndPostcode = doesHNHaveAPostcodeViewModel?.HasPostcode,
                Address = address,
                EcDetails = ecDetails,
                HeatNetworkType = hnType,
                EcSuppliesOneCommunalBuilding = null, // replaced by EnergyCentreType, so not needed
                HasOwnEnergyCenter = hasOwnEc,
                HeatNetworkConnections = heatNetworkConnections,
                NoOfResponsibleCommunalBuilding = communalBuildingResponsibilityViewModel?.ResponsibleCommunalNetworks,
                NoOfResponsibleDistrictNetwork = districtNetworksResponsibilityViewModel?.ResponsibleDistrictNetworks,
                Pathway = viewModel?.PathwayModel?.Pathway,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                RegistrationSource = RegistrationSource.HNTAS,
                Phase = viewModel?.HeatNetworkPhaseModel?.HeatNetworkPhase
            };

            HeatNetworkResponse heatNetworkResponse;

            heatNetworkResponse = await _heatNetworkService.AddHeatNetwork(model);            

            if (heatNetworkResponse?.HnId != null)
            {
                await _organisationService.UpdateOrgHeatNetworkId(orgId, userId, heatNetworkResponse.HnId);
                TempData["Confirmation_HN_Id"] = heatNetworkResponse.HnId;
                TempData["HNName"] = heatNetworkResponse.Name;
                TempData["AdditionalDescription"] = heatNetworkResponse.AdditionalDescription;
                // safe to save HnId in session at this point as it maybe used for redirection to add hn details after registration
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnId, heatNetworkResponse.HnId);
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnName, heatNetworkResponse.Name);
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later";
                return View("HeatNetworkRegistration/CheckYourAnswers", viewModel);
            }
            _sessionHelper.ClearAllHNRegistrationFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);            
            return RedirectToAction("HeatNetworkRegistrationComplete");
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkRegistrationComplete()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HNId = TempData["Confirmation_HN_Id"] as string;
            var hnName = TempData["HNName"] as string;
            var additionalDescription = TempData["AdditionalDescription"] as string;
            ViewBag.HNNameWithDescription = hnName + (!string.IsNullOrEmpty(additionalDescription) ? ", " + additionalDescription : "");
            return View("HeatNetworkRegistration/HeatNetworkRegistrationComplete");
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkSuccessRedirection()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HNId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HNName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);            
            ViewBag.RegistrationSource = RegistrationSource.HNTAS;

            HeatNetworkResponse heatnetwork = await _heatNetworkService.GetAsync(ViewBag.HNId);

            string notificationMessage = string.Empty;

            var communalCount = heatnetwork.NoOfResponsibleCommunalBuilding ?? 0;
            var districtCount = heatnetwork.NoOfResponsibleDistrictNetwork ?? 0;

            if (communalCount > 0 && districtCount > 0)
            {
                notificationMessage = $"You told us your network has {communalCount} new communal buildings and {districtCount} other new district networks. You need to register these separately. We added these to your heat network list for you to complete.";
            }
            else if (communalCount > 0)
            {
                notificationMessage = $"You told us your network has {communalCount} new communal buildings. You need to register these separately. We added these to your heat network list for you to complete.";
            }
            else if (districtCount > 0)
            {
                notificationMessage = $"You told us you have {districtCount} other new district networks. You need to register these separately. We added these to your heat network list for you to complete.";
            }
            else if (heatnetwork.NetworkBuildStatus == NullableOfNetworkBuildStatus.Existing ||
                     heatnetwork.NetworkBuildStatus == NullableOfNetworkBuildStatus.NewAndExisting)
            {
                notificationMessage = "You told us you have existing network connections. Those parts of the network already supplying customers must also be registered separately with <a href=\"https://www.ofgem.gov.uk/energy-regulation/low-carbon/heat-networks\" class=\"govuk-link\" target=\"_blank\">Ofgem</a>.";
            }

            ViewBag.NotificationMessage = notificationMessage;
            var model = _sessionHelper.GetFromSession<HeatNetworkSuccessRedirection>(HttpContext, SessionKeys.HeatNetworkSuccessRedirectionSessionKey) ?? new HeatNetworkSuccessRedirection();
            return View("HeatNetworkRegistration/HeatNetworkSuccessRedirection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkSuccessRedirection(HeatNetworkSuccessRedirection model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid) {
                return View("HeatNetworkRegistration/HeatNetworkSuccessRedirection", model);
            }
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.HeatNetworkSuccessRedirectionSessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            switch (model.NextAction)
            {
                case "HNDetails":
                    return RedirectToAction("AddNetworkDetails", "HeatNetwork", new { hnid = hnId });
                case "AddHN":
                    return RedirectToAction("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
                case "Dashboard":
                default:
                    return RedirectToAction("UserAccount", "Dashboard");
            }
        }
    }
}