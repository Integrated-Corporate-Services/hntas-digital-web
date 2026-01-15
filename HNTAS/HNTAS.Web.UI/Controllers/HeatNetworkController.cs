using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
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
        public IActionResult EnterHNName()
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            return View(heatNetworkNameModel);
        }

        [HttpPost]
        public IActionResult EnterHNName(HeatNetworkNameModel model)
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey, model);
            return RedirectToAction("DoesHNHaveAPostcode");
        }

        [HttpGet]
        public IActionResult DoesHNHaveAPostcode()
        {
            this.ShowBackButton("EnterHNName", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelSessionKey) ?? new DoesHNHaveAPostcodeViewModel { HasPostcode = false };
            return View(model);
        }        

        [HttpPost]
        public async Task<IActionResult> DoesHNHaveAPostcode(DoesHNHaveAPostcodeViewModel model)
        {
            this.ShowBackButton("EnterHNName", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelSessionKey, model);
            if (!model.HasPostcode)
            {
                return RedirectToAction("HNAddressByCoordinates");
            }            
            SearchAddressByPostcodeModel results = await _addressLookUpService.PostcodeLookupAsync(model.Postcode);
            model.Postcode = model.Postcode?.ToUpperInvariant().Trim();
            if (results == null || results.Addresses == null || results.Addresses.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode.");
                return View(model);
            }            
            results.Addresses = results.Addresses
                .Select(address => Utility.CapitalizeCommaSeparated(address))
                .ToArray();
            _sessionHelper.SaveToSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey, results);
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.PreviousStepKey, "HeatNetwork");
            return RedirectToAction("SearchByPostcodeResults", "Address");
        }

        // -- Flow switching to AddressController for postcode search results -- //
        // -- Flow switching back from AddressController after address selection -- //

        [HttpGet]
        public IActionResult SaveHNAddressByPostcode()
        {
            var heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey);
            if (model == null)
            {                
                return BadRequest("Missing session data");
            }

            heatNetworkLocationModel.HNAddressByStreet = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);
            return RedirectToAction("HNAddressByCoordinates", "HeatNetwork");
        }

        [HttpGet]
        public IActionResult AddressManualEntry()
        {
            this.ShowBackButton("EnterHNName", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey) ?? new AddressByStreetOrTownModel { Country = "United Kingdom" };
            return View(model);
        }

        [HttpPost]
        public IActionResult AddressManualEntry(AddressByStreetOrTownModel model)
        {
            this.ShowBackButton("EnterHNName", "HeatNetwork");
            var heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            var addressParts = new[] { model.StreetAddress, model.TownOrCity, model.Postalcode, model.Country }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            model.Fulladdress = string.Join(", ", addressParts);
            heatNetworkLocationModel.HNAddressByStreet = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);
            return RedirectToAction("HNAddressByCoordinates", "HeatNetwork");
        }

        [HttpGet]
        public IActionResult HNAddressByCoordinates()
        {
            this.ShowBackButton("DoesHNHaveAPostcode");
            var model = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HNAddressByCoordinates(HeatNetworkLocationModel model)
        {
            // Re-create nested container if null so view rendering doesn't NRE
            if (model?.HNAddressByLatLong == null)
            {
                model ??= new HeatNetworkLocationModel();
                model.HNAddressByLatLong = new AddressByLatLongModel();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Try to split and parse the LatitudeLongitude value
            var raw = model.LatitudeLongitude ?? string.Empty;
            var parts = raw.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            if (parts.Length != 2
                || !decimal.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lat)
                || !decimal.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lon))
            {
                ModelState.AddModelError(nameof(model.LatitudeLongitude),
                    "Enter latitude and longitude in the format: 'latitude, longitude' (for example 52.93430970409369, -1.2522174890632065).");
                return View("HNAddressByCoordinates", model);
            }

            // Populate nested AddressByLatLongModel
            model.HNAddressByLatLong.Latitude = lat;
            model.HNAddressByLatLong.Longitude = lon;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, model);
            return RedirectToAction("EnterHNPhase");
        }        

        [HttpGet]
        public IActionResult EnterHNPhase()
        {
            this.ShowBackButton("HNAddressByCoordinates", "HeatNetwork");
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey) ?? new HeatNetworkPhaseModel();
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            ViewBag.HNName = heatNetworkNameModel.HeatNetworkName;
            return View("EnterHNPhase", heatNetworkPhaseModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnterHNPhase(HeatNetworkPhaseModel model)
        {
            this.ShowBackButton("HNAddressByCoordinates", "HeatNetwork");
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
                    case "design":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers");
                    case "construction":
                        return RedirectToAction("HaveYouSignedMEContract");
                    case "operation":
                        return RedirectToAction("HNInOperation");
                    default:
                        ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase.");
                        return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult HaveYouSignedMEContract()
        {
            this.ShowBackButton("EnterHNPhase", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<HaveYouSignedMEContractModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey) ?? new HaveYouSignedMEContractModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HaveYouSignedMEContract(HaveYouSignedMEContractModel model)
        {
            this.ShowBackButton("EnterHNPhase", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.HaveYouSignedMEContract)
            {
                case "yes":
                    _sessionHelper.SaveToSession<HaveYouSignedMEContractModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model);
                    return RedirectToAction("MEContractIsSigned", "HeatNetwork");
                case "no":
                    _sessionHelper.SaveToSession<HaveYouSignedMEContractModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model);
                    return RedirectToAction("HasElementBeenRegistered", "HeatNetwork");
                default:
                    ModelState.AddModelError(nameof(model.HaveYouSignedMEContract), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult MEContractIsSigned()
        {
            this.ShowBackButton("HaveYouSignedMEContract", "HeatNetwork");
            return View();
        }

        [HttpGet]
        public IActionResult HasElementBeenRegistered()
        {
            this.ShowBackButton("HaveYouSignedMEContract", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<HasElementBeenRegisteredModel>(HttpContext, SessionKeys.HasElementBeenRegisteredModelKey) ?? new HasElementBeenRegisteredModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HasElementBeenRegistered(HasElementBeenRegisteredModel model)
        {
            this.ShowBackButton("HaveYouSignedMEContract", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (string.IsNullOrWhiteSpace(model.HasElementBeenRegistered))
            {
                ModelState.AddModelError(nameof(model.HasElementBeenRegistered), "Please select an option.");
                return View(model);
            }
            else
            {
                _sessionHelper.SaveToSession<HasElementBeenRegisteredModel>(HttpContext, SessionKeys.HasElementBeenRegisteredModelKey, model);
                switch (model.HasElementBeenRegistered)
                {
                    case "yes":
                        return RedirectToAction("HasPlanningApplicationBeenSubmitted");
                    case "no":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers");
                    default:
                        ModelState.AddModelError(nameof(model.HasElementBeenRegistered), "Please select an option.");
                        return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult HasPlanningApplicationBeenSubmitted()
        {
            this.ShowBackButton("HasElementBeenRegistered", "HeatNetwork");
            var model = _sessionHelper.GetFromSession<HasPlanningApplicationBeenSubmittedModel>(HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey) ?? new HasPlanningApplicationBeenSubmittedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HasPlanningApplicationBeenSubmitted(HasPlanningApplicationBeenSubmittedModel model)
        {
            this.ShowBackButton("HasElementBeenRegistered", "HeatNetwork");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (string.IsNullOrWhiteSpace(model.HasPlanningApplicationBeenSubmitted))
            {
                ModelState.AddModelError(nameof(model.HasPlanningApplicationBeenSubmitted), "Please select an option.");
                return View(model);
            }
            else
            {
                _sessionHelper.SaveToSession<HasPlanningApplicationBeenSubmittedModel>(HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey, model);
                switch (model.HasPlanningApplicationBeenSubmitted)
                {
                    case "yes":
                        //store pathway as 3, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "3" });
                        return RedirectToAction("CheckYourAnswers");
                    case "no":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers");
                    default:
                        ModelState.AddModelError(nameof(model.HasPlanningApplicationBeenSubmitted), "Please select an option.");
                        return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult HNInOperation()
        {
            this.ShowBackButton("EnterHNPhase", "HeatNetwork");
            return View();
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            ViewBag.ShowBackButton = false;

            if (_sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) == null)
            {
                return RedirectToAction("UserAccount", "Dashboard");
            }

            var model = new CheckYourAnswersHeatNetworkModel
            {
                HeatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey),
                HeatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey),
                HeatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey),
                HaveYouSignedMEContractModel = _sessionHelper.GetFromSession<HaveYouSignedMEContractModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey) ?? new HaveYouSignedMEContractModel(),
                HasElementBeenRegisteredModel = _sessionHelper.GetFromSession<HasElementBeenRegisteredModel>(HttpContext, SessionKeys.HasElementBeenRegisteredModelKey) ?? null,
                HasPlanningApplicationBeenSubmittedModel = _sessionHelper.GetFromSession<HasPlanningApplicationBeenSubmittedModel>(HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey) ?? null,
                PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey) ?? new PathwayModel() { Pathway = "1" },
                ConfirmedDeclaration = false
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(CheckYourAnswersHeatNetworkModel viewModel)
        {

            viewModel.HeatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            viewModel.HeatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            viewModel.PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey);
            viewModel.HeatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey);
            viewModel.HaveYouSignedMEContractModel = _sessionHelper.GetFromSession<HaveYouSignedMEContractModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey) ?? null;
            viewModel.HasElementBeenRegisteredModel = _sessionHelper.GetFromSession<HasElementBeenRegisteredModel>(HttpContext, SessionKeys.HasElementBeenRegisteredModelKey) ?? null;
            viewModel.HasPlanningApplicationBeenSubmittedModel = _sessionHelper.GetFromSession<HasPlanningApplicationBeenSubmittedModel>(HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey) ?? null;

            ModelState.Remove(nameof(viewModel.HeatNetworkNameModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkLocationModel));
            ModelState.Remove(nameof(viewModel.PathwayModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkPhaseModel));
            ModelState.Remove(nameof(viewModel.HaveYouSignedMEContractModel));
            ModelState.Remove(nameof(viewModel.HasElementBeenRegisteredModel));
            ModelState.Remove(nameof(viewModel.HasPlanningApplicationBeenSubmittedModel));


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


            var model = new HeatNetwork
            {
                Name = viewModel?.HeatNetworkNameModel?.HeatNetworkName,
                Location = viewModel?.HeatNetworkLocationModel?.LatitudeLongitude,
                Pathway = viewModel?.PathwayModel?.Pathway,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            var userResponse = await _heatNetworkService.AddHeatNetwork(model);

            if (userResponse.HnId != null)
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

            return RedirectToAction("Confirmation", "HeatNetwork");
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            var userResponse = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));

            ViewBag.CompanyName = userResponse.Organisation?.Name;
            ViewBag.ContactName = userResponse.FullName;
            ViewBag.HNId = TempData["Confirmation_HN_Id"] as string;
            ViewBag.HNName = TempData["HNName"] as string;
            return View("Confirmation");
        }

        [HttpGet]
        public async Task<IActionResult> Details([FromQuery] string hnid)
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            // get user details
            var response = await _heatNetworkService.GetAsync(hnid?.ToUpper());

            if (response == null)
            {
                return BadRequest();
            }

            var model = new HNDetailsViewModel
            {
                Name = response?.Name,
                LocationUrl = response?.Location,
                OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName),
                PathWay = response.Pathway,
                UHNID = response?.HnId
            };

            return View(model);

        }

        [HttpPost]
        public IActionResult SubmitDetails(HNDetailsViewModel model)
        {
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, model.UHNID);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, model.Name);
            return RedirectToAction("SOAIntro", "SOA");
        }

    }
}
