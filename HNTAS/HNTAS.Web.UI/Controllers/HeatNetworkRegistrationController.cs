using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
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
        private readonly ILogger<HeatNetworkRegistrationController> _logger;

        public HeatNetworkRegistrationController(ISessionHelper sessionHelper, IHeatNetworkService heatNetworkService, IOrganisationService organisationService, IAddressLookupService addressLookupService, ILogger<HeatNetworkRegistrationController> logger)
        {
            _sessionHelper = sessionHelper;
            _heatNetworkService = heatNetworkService;
            _organisationService = organisationService;
            _addressLookUpService = addressLookupService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult HeatNetworkDwellingsCheck()
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            this.ShowBackButton("HeatNetworkDwellingsCheck");
            return View();
        }


        #region change in progress

        // check session save
        // check navigation to correct next page based on selection and session values
        // allot proper hn type and connections values in session for correct next pages to read and display summary
        // for communal with integral EC -> save hn type, save connections with all false and null counts, navigate to communal EC summary

        [HttpGet]
        public IActionResult HeatNetworkType()
        {
            this.ShowBackButton("HeatNetworkIntroduction");
            var model = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel) ?? new IsHnTypeCommunalViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkType(IsHnTypeCommunalViewModel model)
        {
            this.ShowBackButton("HeatNetworkIntroduction");
            if (!ModelState.IsValid)
            {
                return View(model);
            }            
            string nextAction = model.IsHnTypeCommunal switch
            {
                true => "HeatNetworkEcCommunal",
                false => "HeatNetworkEcDistrict"
            };            
            _sessionHelper.SaveToSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel, model);
            return RedirectToAction(nextAction);
        }

        // Communal or district
        // for communal
        // HeatNetworkEcCommunal, If yes HeatNetworkCommunalOneBlock, If no HeatNetworkCommunalNoEcSummary
        // If yes HeatNetworkCommunalOneBlock, If yes HeatNetworkCommunalEcSummary, If no HeatNetworkCommunalOneBlockSummary        
        // both converge at HeatNetworkName

        

        [HttpGet]
        public IActionResult HeatNetworkEcCommunal()
        {
            this.ShowBackButton("HeatNetworkType");
            var model = _sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel) ?? new DoesCommunalHnHaveOwnEcViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkEcCommunal(DoesCommunalHnHaveOwnEcViewModel model)
        {
            this.ShowBackButton("HeatNetworkType");
            if (!ModelState.IsValid)
            {
                return View();
            }
            string nextAction = model.HasOwnEc switch
            {
                true => "HeatNetworkCommunalOneBlock",
                false => "HeatNetworkCommunalNoECSummary",
            };
            _sessionHelper.SaveToSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel, model);
            return RedirectToAction(nextAction);
        }
            

        [HttpGet]
        public IActionResult HeatNetworkCommunalOneBlock()
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            var model = _sessionHelper.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(HttpContext, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel) ?? new DoesCommunalEcSupplyOneBlockViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkCommunalOneBlock(DoesCommunalEcSupplyOneBlockViewModel model)
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            if(!ModelState.IsValid)
            {
                return View();
            }
            string nextAction = model.SuppliesOneBlock switch
            {
                true => "HeatNetworkCommunalECSummary",
                false => "HeatNetworkCommunalOneBlockSummary"
            };
            _sessionHelper.SaveToSession<DoesCommunalEcSupplyOneBlockViewModel>(HttpContext, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel, model);
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalECSummary()
        {
            this.ShowBackButton("HeatNetworkCommunalOneBlock");
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalOneBlockSummary()
        {
            this.ShowBackButton("HeatNetworkCommunalOneBlock");
            return View();
        }        

        [HttpGet]
        public IActionResult HeatNetworkCommunalNoECSummary()
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            return View();
        }

        // for district
        // HeatNetworkEcDistrict
        // if yes HeatNetworkConnections
        // HeatNetworkDistrictEcSummary
        // if no HeatNetworkConnections (different options)
        // HeatNetworkDistrictNoEcSummary
        // both converge at HeatNetworkName
        [HttpGet]
        public IActionResult HeatNetworkEcDistrict()
        {
            this.ShowBackButton("HeatNetworkType");
            var model = _sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel) ?? new DoesDistrictHnHaveOwnEcViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkEcDistrict(string hasEC)
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            if (hasEC == "yes")
            {
                // options
                
            }
            else if (hasEC == "no")
            {
                // options
                return RedirectToAction("HeatNetworkConnections");
            }
            return RedirectToAction("HeatNetworkConnections");
        }

        //[HttpGet]

        //public IActionResult HeatNetworkConnections()
        //{
            
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult HeatNetworkConnections(HeatNetworkConnectionsViewModel model)
        //{
            

        //}

        [HttpGet]
        public IActionResult HeatNetworkDistrictEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View(model);
        }
        [HttpGet]
        public IActionResult HeatNetworkDistrictNoEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View(model);
        }


#endregion


        [HttpGet]
        public IActionResult HeatNetworkName()
        {
            //var backAction = hnTypeModel.HeatNetworkType switch
            //{
            //    Models.Enums.HeatNetworkType.CommunalWithIntegralEC => "HeatNetworkCommunalECSummary",
            //    Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN => "HeatNetworkCommunalNoECSummary",
            //    Models.Enums.HeatNetworkType.DistrictWithOwnEC => "HeatNetworkDistrictEcSummary",
            //    Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN => "HeatNetworkDistrictNoEcSummary",
            //    _ => "HeatNetworkSummary"
            //};
            //this.ShowBackButton(backAction);
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();
            return View(heatNetworkNameModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkName(HeatNetworkNameModel model)
        {
            //var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            //var backAction = hnTypeModel.HeatNetworkType switch
            //{
            //    Models.Enums.HeatNetworkType.CommunalWithIntegralEC => "HeatNetworkCommunalECSummary",
            //    Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN => "HeatNetworkCommunalNoECSummary",
            //    Models.Enums.HeatNetworkType.DistrictWithOwnEC => "HeatNetworkDistrictEcSummary",
            //    Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN => "HeatNetworkDistrictNoEcSummary",
            //    _ => "HeatNetworkSummary"
            //};
            //this.ShowBackButton(backAction);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey, model);
            //if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN)
            //{
            //    return RedirectToAction("ECCoordinates");
            //}
            //else
            //{
                return RedirectToAction("DoesHNHaveAPostcode");
            //}                
        }

        #region address input
        [HttpGet]
        public IActionResult DoesHNHaveAPostcode()
        {            
            this.ShowBackButton("HeatNetworkName");
            var model = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey) ?? new DoesHNHaveAPostcodeViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoesHNHaveAPostcode(DoesHNHaveAPostcodeViewModel model)
        {
            this.ShowBackButton("HeatNetworkName");

            if (!ModelState.IsValid)
            {
                return View(model);
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
                    ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode.");
                    return View(model);
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

            SearchAddressByPostcodeModel model = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);

            if (model == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel is null.");
                return View("DoesHNHaveAPostcode");
            }
            return View(model);
        }

        public IActionResult SelectAddress(string selectedAddress)
        {
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
            var model = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey)?.HNAddressByStreet ?? new AddressByStreetOrTownModel { Country = "United Kingdom" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddressManualEntry(AddressByStreetOrTownModel model)
        {
            this.ShowBackButton("HeatNetworkName");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var addressParts = new[] { model.StreetAddress, model.TownOrCity, model.Postalcode, model.Country }
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
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAddress(AddressByStreetOrTownModel model)
        {
            return RedirectToAction("SaveHNAddressByPostcode");
        }
        #endregion

        [HttpGet]
        public IActionResult SaveHNAddressByPostcode()
        {
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey) ?? new AddressByStreetOrTownModel();            
            var doesHnHaveAPostcodeModel = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel;
            if ((bool)doesHnHaveAPostcodeModel?.HasPostcode!)
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
            //var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            //if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN)
            //{
            //    this.ShowBackButton("HeatNetworkName");
            //}
            //else
            //{
            //    this.ShowBackButton("DoesHNHaveAPostcode");
            //}            
            
            var model = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey) ?? new ECDetailsModel { ECAddressByLatLong = new AddressByLatLongModel() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ECCoordinates(ECDetailsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Try to split and parse the LatitudeLongitude value
            var raw = model.LatitudeLongitude;
            var parts = raw.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            if (parts.Length != 2
                || !decimal.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lat)
                || !decimal.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lon))
            {
                ModelState.AddModelError(nameof(model.LatitudeLongitude),
                    "Enter latitude and longitude in correct format.");
                return View("ECCoordinates", model);
            }

            // Populate nested AddressByLatLongModel
            model.ECAddressByLatLong.Latitude = lat;
            model.ECAddressByLatLong.Longitude = lon;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ECDetailsModelSessionKey, model);
            return RedirectToAction("HeatNetworkPhase");
        }
        #endregion

        

        [HttpGet]
        public IActionResult HeatNetworkPhase()
        {
            this.ShowBackButton("ECCoordinates");
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey) ?? new HeatNetworkPhaseModel();            
            return View("HeatNetworkPhase", heatNetworkPhaseModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkPhase(HeatNetworkPhaseModel model)
        {
            this.ShowBackButton("ECCoordinates");
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
                    case "Feasibility":
                        // store pathway as 1, navigate to cya
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    case "Design":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "2" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");                    
                    case "Construction":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "3" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");                    
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
            //HeatNetworkTypeViewModel heatNetworkTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            HeatNetworkConnectionsViewModel heatNetworkConnectionsModel = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            //if (heatNetworkNameModel == null || heatNetworkPhaseModel == null || heatNetworkTypeModel == null || heatNetworkConnectionsModel == null)
            //{
            //    return RedirectToAction("UserAccount", "Dashboard");
            //}

            var model = new CheckYourAnswersHeatNetworkModel
            {
                HeatNetworkNameModel = heatNetworkNameModel,
                HeatNetworkAddressModel = heatNetworkLocationModel?.HNAddressByStreet ?? null,
                ECDetailsModel = ecDetailsModel,
                HeatNetworkPhaseModel = heatNetworkPhaseModel,
                //HeatNetworkTypeModel = heatNetworkTypeModel,
                HeatNetworkConnectionsModel = heatNetworkConnectionsModel,                
                PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey) ?? new PathwayModel() { Pathway = "1" },
                ConfirmedDeclaration = false
            };

            _sessionHelper.SaveToSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(bool ConfirmedDeclaration)
        {
            var viewModel = _sessionHelper.GetFromSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey);
            ModelState.Remove(nameof(viewModel.HeatNetworkNameModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkAddressModel));
            ModelState.Remove(nameof(viewModel.ECDetailsModel));
            ModelState.Remove(nameof(viewModel.PathwayModel));
            ModelState.Remove(nameof(viewModel.HeatNetworkPhaseModel));

            // Validate the mandatory checkbox
            if (ConfirmedDeclaration != true)
            {
                ModelState.AddModelError(nameof(viewModel.ConfirmedDeclaration), "You must confirm the declaration to proceed.");
            }

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
            //HNTAS.Api.Client.Model.HeatNetworkType hnType = viewModel?.HeatNetworkTypeModel?.HeatNetworkType switch
            //{
            //    HNTAS.Web.UI.Models.Enums.HeatNetworkType.CommunalWithIntegralEC => HNTAS.Api.Client.Model.HeatNetworkType.CommunalWithIntegralEC,
            //    HNTAS.Web.UI.Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN => HNTAS.Api.Client.Model.HeatNetworkType.CommunalWithSeparateUpstreamHN,
            //    HNTAS.Web.UI.Models.Enums.HeatNetworkType.DistrictWithOwnEC => HNTAS.Api.Client.Model.HeatNetworkType.DistrictWithOwnEC,
            //    HNTAS.Web.UI.Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN => HNTAS.Api.Client.Model.HeatNetworkType.DistrictWithSeparateUpstreamHN,
            //};
            HNTAS.Api.Client.Model.HeatNetworkConnections heatNetworkConnections = new HNTAS.Api.Client.Model.HeatNetworkConnections
            {
                IsCommunalBuilding = viewModel?.HeatNetworkConnectionsModel?.IsCommunalBuilding,
                NoOfCommunalBuilding = viewModel?.HeatNetworkConnectionsModel?.NoOfCommunalBuilding,
                IsDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.IsDomesticConsumer,
                NoOfDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.NoOfDomesticConsumer,
                IsNonDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.IsNonDomesticConsumer,
                NoOfNonDomesticConsumer = viewModel?.HeatNetworkConnectionsModel?.NoOfNonDomesticConsumer,                
                IsDownstreamDistrictHeatNetworkConnections = viewModel?.HeatNetworkConnectionsModel?.IsDownstreamDistrictHeatNetworkConnections,
                NoOfDownstreamDistrictHeatNetworkConnections = viewModel?.HeatNetworkConnectionsModel?.NoOfDownstreamDistrictHeatNetworkConnections
            };

            var model = new HeatNetwork
            {
                Name = viewModel?.HeatNetworkNameModel?.HeatNetworkName,
                AdditionalDescription = viewModel?.HeatNetworkNameModel?.AdditionalDescription,
                Address = address,
                EcDetails = ecDetails,
                //HeatNetworkType = hnType,
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
                TempData["AdditionalDescription"] = userResponse.AdditionalDescription;
                // safe to save HnId in session at this point as it maybe used for redirection to add hn details after registration
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnId, userResponse.HnId);
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnName, userResponse.Name);
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later.";
                return View("CheckYourAnswers", viewModel);
            }
            _sessionHelper.ClearAllHNRegistrationFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);            
            return RedirectToAction("HeatNetworkRegistrationComplete");
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkRegistrationComplete()
        {             
            ViewBag.HNId = TempData["Confirmation_HN_Id"] as string;
            var hnName = TempData["HNName"] as string;
            var additionalDescription = TempData["AdditionalDescription"] as string;
            ViewBag.HNNameWithDescription = hnName + (!string.IsNullOrEmpty(additionalDescription) ? ", " + additionalDescription : "");
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkSuccessRedirection()
        {
            ViewBag.HNId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HNName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var model = _sessionHelper.GetFromSession<HeatNetworkSuccessRedirection>(HttpContext, SessionKeys.HeatNetworkSuccessRedirectionSessionKey) ?? new HeatNetworkSuccessRedirection();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkSuccessRedirection(HeatNetworkSuccessRedirection model)
        {
            if (!ModelState.IsValid) {
                return View(model);
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
