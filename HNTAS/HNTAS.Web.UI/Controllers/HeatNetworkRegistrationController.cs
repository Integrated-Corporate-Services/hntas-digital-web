using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkRegistrationController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IOrganisationService _organisationService;

        public HeatNetworkRegistrationController(ISessionHelper sessionHelper, IHeatNetworkService heatNetworkService, IOrganisationService organisationService)
        {
            _sessionHelper = sessionHelper;
            _heatNetworkService = heatNetworkService;
            _organisationService = organisationService;
        }

        #region wip
        [HttpGet]
        public IActionResult HeatNetworkDwellingsCheck()
        {
            this.ShowBackButton("HeatNetworksAsync", "UserManagement");
            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult HeatNetworkDwellingsCheck(HowManyDwellingsIncludedModel model)
        {
            this.ShowBackButton("HeatNetworksAsync", "UserManagement");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
            switch (model.HowManyDwellingsIncluded)
            {
                case "yes":
                    return RedirectToAction("HeatNetworkIntroduction");
                case "no":
                    return RedirectToAction("SixOrMoreDwellingsAnswerNo");
                default:
                    ModelState.AddModelError(nameof(model.HowManyDwellingsIncluded), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult SixOrMoreDwellingsAnswerNo()
        {
            this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkIntroduction()
        {
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkType()
        {
            this.ShowBackButton("HeatNetworkIntroduction", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey) ?? new HeatNetworkTypeViewModel();
            return View(model);
        }

        #endregion

        [HttpPost]
        public IActionResult HeatNetworkType(HeatNetworkTypeViewModel model)
        {
            this.ShowBackButton("HeatNetworkIntroduction", "HeatNetworkRegistration");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string nextAction;
            switch (model.HeatNetworkType)
            {
                case Models.Enums.HeatNetworkType.CommunalWithIntegralEC:
                    nextAction = "HeatNetworkCommunalECSummary";
                    break;
                case Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN:
                    nextAction = "HeatNetworkCommunalNoECSummary";
                    break;
                case Models.Enums.HeatNetworkType.DistrictWithOwnEC:
                case Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN:
                    nextAction = "HeatNetworkConnections";
                    break;
                default:
                    ModelState.AddModelError(nameof(model.HeatNetworkType), "Please select a valid option.");
                    return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey, model);
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalECSummary()
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalNoECSummary()
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            return View();
        }

        [HttpGet]

        public IActionResult HeatNetworkConnections()
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            var newModel = new HeatNetworkConnectionsViewModel();
            if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithOwnEC)
            {
                ViewBag.IsDistrictWithOwnEC = true;
                newModel.IsUpstreamDistrictHeatNetworkConnections = false;
            }
            else if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN)
            {
                ViewBag.IsDistrictWithOwnEC = false;
                newModel.IsDownstreamDistrictHeatNetworkConnections = false;
            }
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey) ?? newModel;
            return View(model);
        }

        [HttpPost]
        public IActionResult HeatNetworkConnections(HeatNetworkConnectionsViewModel model)
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");

            if (model.IsCommunalBuilding && !model.NoOfCommunalBuilding.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfCommunalBuilding), "Enter the number of communal buildings");
            }

            if (model.IsDomesticConsumer && !model.NoOfDomesticConsumer.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfDomesticConsumer), "Enter the number of domestic consumers");
            }

            if (model.IsNonDomesticConsumer && !model.NoOfNonDomesticConsumer.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfNonDomesticConsumer), "Enter the number of non-domestic consumers");
            }

            if (model.IsDownstreamDistrictHeatNetworkConnections && !model.NoOfDownstreamDistrictHeatNetworkConnections.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfDownstreamDistrictHeatNetworkConnections), "Enter the number of district connections");
            }

            if (model.IsUpstreamDistrictHeatNetworkConnections && !model.NoOfUpstreamDistrictHeatNetworkConnections.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfUpstreamDistrictHeatNetworkConnections), "Enter the number of district connections");
            }

            if (!model.IsCommunalBuilding && !model.IsDomesticConsumer && !model.IsNonDomesticConsumer && !model.IsUpstreamDistrictHeatNetworkConnections && !model.IsDownstreamDistrictHeatNetworkConnections)
            {
                // Model-level error (not associated with a specific property)
                ModelState.AddModelError("HeatNetworkConnections", "Select at least one connection type");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey, model);
            var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithOwnEC)
            {
                return RedirectToAction("HeatNetworkDistrictEcSummary");
            }
            else
            {
                return RedirectToAction("HeatNetworkDistrictNoEcSummary");
            }
        }

        public IActionResult HeatNetworkDistrictEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View(model);
        }

        public IActionResult HeatNetworkDistrictNoEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View(model);
        }

        [HttpGet]
        public IActionResult HeatNetworkSummary()
        {
            return RedirectToAction("HeatNetworkName", "HeatNetworkRegistration");
        }

        [HttpGet]
        public IActionResult HeatNetworkName()
        {
            var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            var backAction = hnTypeModel.HeatNetworkType switch
            {
                Models.Enums.HeatNetworkType.CommunalWithIntegralEC => "HeatNetworkCommunalECSummary",
                Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN => "HeatNetworkCommunalNoECSummary",
                Models.Enums.HeatNetworkType.DistrictWithOwnEC => "HeatNetworkDistrictEcSummary",
                Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN => "HeatNetworkDistrictNoEcSummary",
                _ => "HeatNetworkSummary"
            };
            this.ShowBackButton(backAction);
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            return View(heatNetworkNameModel);
        }

        [HttpPost]
        public IActionResult HeatNetworkName(HeatNetworkNameModel model)
        {
            var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            var backAction = hnTypeModel.HeatNetworkType switch
            {
                Models.Enums.HeatNetworkType.CommunalWithIntegralEC => "HeatNetworkCommunalECSummary",
                Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN => "HeatNetworkCommunalNoECSummary",
                Models.Enums.HeatNetworkType.DistrictWithOwnEC => "HeatNetworkDistrictEcSummary",
                Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN => "HeatNetworkDistrictNoEcSummary",
                _ => "HeatNetworkSummary"
            };
            this.ShowBackButton(backAction);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey, model);
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.PreviousStepKey, "HeatNetworkRegistration");
            return RedirectToAction("DoesHNHaveAPostcode", "Address");
        }

        [HttpGet]
        public IActionResult SaveHNAddressByPostcode()
        {
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey) ?? new AddressByStreetOrTownModel();
            if (model == null)
            {
                return BadRequest("Missing session data");
            }
            var heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel { HNAddressByStreet = new AddressByStreetOrTownModel() };
            heatNetworkLocationModel.HNAddressByStreet = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);
            return RedirectToAction("ECCoordinates", "Coordinates");
        }

        [HttpGet]
        public IActionResult HeatNetworkPhase()
        {
            this.ShowBackButton("ECCoordinates", "Coordinates");
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey) ?? new HeatNetworkPhaseModel();            
            return View("HeatNetworkPhase", heatNetworkPhaseModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkPhase(HeatNetworkPhaseModel model)
        {
            this.ShowBackButton("ECCoordinates", "Coordinates");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (string.IsNullOrWhiteSpace(model.HeatNetworkPhase))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase.");
                return View(model);
            }
            else
            {
                _sessionHelper.SaveToSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model);
                switch (model.HeatNetworkPhase)
                {
                    case "Design":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "2" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    case "Feasibility":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    case "Construction":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "3" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    case "Operation":
                        return RedirectToAction("HNInOperation");
                    default:
                        ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase.");
                        return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult HeatNetworkLeaveService()
        {
            this.ShowBackButton("HeatNetworkPhase");
            return View();
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            ViewBag.ShowBackButton = false;           

            HeatNetworkNameModel heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            ECDetailsModel ecDetailsModel = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey);
            HeatNetworkPhaseModel heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey);
            HeatNetworkTypeViewModel heatNetworkTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            HeatNetworkConnectionsViewModel heatNetworkConnectionsModel = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            if (heatNetworkNameModel == null || heatNetworkPhaseModel == null || heatNetworkTypeModel == null || heatNetworkConnectionsModel == null)
            {
                return RedirectToAction("UserAccount", "Dashboard");
            }

            var model = new CheckYourAnswersHeatNetworkModel
            {
                HeatNetworkNameModel = heatNetworkNameModel,
                HeatNetworkAddressModel = heatNetworkLocationModel?.HNAddressByStreet ?? null,
                ECDetailsModel = ecDetailsModel,
                HeatNetworkPhaseModel = heatNetworkPhaseModel,
                HeatNetworkTypeModel = heatNetworkTypeModel,
                HeatNetworkConnectionsModel = heatNetworkConnectionsModel,                
                PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey) ?? new PathwayModel() { Pathway = "1" },
                ConfirmedDeclaration = false
            };

            _sessionHelper.SaveToSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers()
        {
            var viewModel = _sessionHelper.GetFromSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey);
            ModelState.Remove(nameof(viewModel.HeatNetworkNameModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkAddressModel));
            ModelState.Remove(nameof(viewModel.ECDetailsModel));
            ModelState.Remove(nameof(viewModel.PathwayModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkPhaseModel));
            if (!ModelState.IsValid)
            {
                return View("CheckYourAnswers", viewModel);
            }

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            if (userId == null || orgId == null)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later.";
                return View("CheckYourAnswers", viewModel);
            }

            var hnAddress = viewModel?.HeatNetworkAddressModel;

            double? latitude = null;
            double? longitude = null;
            if (viewModel?.ECDetailsModel?.ECAddressByLatLong != null)
            {
                latitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Latitude;
                longitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Longitude;
            }

            ECDetails2? ecDetails = (latitude.HasValue || longitude.HasValue)
                ? new ECDetails2(latitude: latitude, longitude: longitude)
                : null;

            var address = viewModel.HeatNetworkAddressModel != null ? new RegisteredAddress(
                    addressLine1: hnAddress?.StreetAddress?.Trim(),
                    postcode: hnAddress?.Postalcode?.Trim(),
                    addressLine2: default,
                    town: hnAddress?.TownOrCity?.Trim(),
                    county: default,
                    country: hnAddress?.Country?.Trim()
                ) : null;
            HNTAS.Api.Client.Model.HeatNetworkType hnType = viewModel?.HeatNetworkTypeModel?.HeatNetworkType switch
            {
                HNTAS.Web.UI.Models.Enums.HeatNetworkType.CommunalWithIntegralEC => HNTAS.Api.Client.Model.HeatNetworkType.CommunalWithIntegralEC,
                HNTAS.Web.UI.Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN => HNTAS.Api.Client.Model.HeatNetworkType.CommunalWithSeparateUpstreamHN,
                HNTAS.Web.UI.Models.Enums.HeatNetworkType.DistrictWithOwnEC => HNTAS.Api.Client.Model.HeatNetworkType.DistrictWithOwnEC,
                HNTAS.Web.UI.Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN => HNTAS.Api.Client.Model.HeatNetworkType.DistrictWithSeparateUpstreamHN,
            };
            HNTAS.Api.Client.Model.HeatNetworkConnections heatNetworkConnections = new HNTAS.Api.Client.Model.HeatNetworkConnections
            {
                IsCommunalBuilding = viewModel?.HeatNetworkConnectionsModel?.IsCommunalBuilding,
                NoOfCommunalBuilding = viewModel?.HeatNetworkConnectionsModel?.NoOfCommunalBuilding,
                IsDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.IsDomesticConsumer,
                NoOfDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.NoOfDomesticConsumer,
                IsNonDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.IsNonDomesticConsumer,
                NoOfNonDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.NoOfNonDomesticConsumer,
                IsUpstreamDistrictHeatNetworkConnections = viewModel?.HeatNetworkConnectionsModel?.IsUpstreamDistrictHeatNetworkConnections,
                NoOfUpstreamDistrictHeatNetworkConnections = viewModel?.HeatNetworkConnectionsModel?.NoOfUpstreamDistrictHeatNetworkConnections,
                IsDownstreamDistrictHeatNetworkConnections = viewModel?.HeatNetworkConnectionsModel?.IsDownstreamDistrictHeatNetworkConnections,
                NoOfDownstreamDistrictHeatNetworkConnections = viewModel?.HeatNetworkConnectionsModel?.NoOfDownstreamDistrictHeatNetworkConnections
            };

            var model = new HeatNetwork
            {
                Name = viewModel?.HeatNetworkNameModel?.HeatNetworkName,
                Address = address,
                EcDetails = ecDetails,
                HeatNetworkType = hnType,
                HeatNetworkConnections = heatNetworkConnections,
                Pathway = viewModel?.PathwayModel?.Pathway,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                RegistrationSource = RegistrationSource.HNTAS,
                Phase = viewModel?.HeatNetworkPhaseModel?.HeatNetworkPhase
            };

            var userResponse = await _heatNetworkService.AddHeatNetwork(model);

            if (userResponse?.HnId != null)
            {
                await _organisationService.UpdateOrgHeatNetworkId(orgId, userId, userResponse.HnId);
                TempData["Confirmation_HN_Id"] = userResponse.HnId;
                TempData["HNName"] = userResponse.Name;
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later.";
                return View("CheckYourAnswers", viewModel);
            }
            _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);

            return RedirectToAction("HeatNetworkRegistrationComplete");
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkRegistrationComplete()
        {             
            ViewBag.HNId = TempData["Confirmation_HN_Id"] as string;
            ViewBag.HNName = TempData["HNName"] as string;
            return View();
        }
    }
}
