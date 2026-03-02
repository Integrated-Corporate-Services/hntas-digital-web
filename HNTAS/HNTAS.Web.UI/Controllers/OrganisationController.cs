using DocumentFormat.OpenXml.EMMA;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text.RegularExpressions;
using PreferredContactType = HNTAS.Web.UI.Models.Enums.PreferredContactType;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class OrganisationController : Controller
    {
        private readonly ICompaniesHouseService _companiesHouseService;
        private readonly IAddressLookupService _addressLookUpService;
        private readonly ILogger<OrganisationController> _logger;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;
        private readonly ICountriesAndTerritoriesService _countriesAndTerritoriesService;
        private readonly IOrganisationService _organisationService;

        public OrganisationController(ICompaniesHouseService companiesHouseService,
            ILogger<OrganisationController> logger,
            IUserService userService,
            ISessionHelper sessionHelper,
            IAddressLookupService addressLookUpService,
            ICountriesAndTerritoriesService countriesAndTerritoriesService,
            IOrganisationService organisationService)
        {
            _companiesHouseService = companiesHouseService;
            _logger = logger;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _addressLookUpService = addressLookUpService;
            _countriesAndTerritoriesService = countriesAndTerritoriesService;
            _organisationService = organisationService;
        }

        public string CapitalizeCommaSeparated(string input)
        {

            if (string.IsNullOrWhiteSpace(input))
                return input;

            var words = input.Split(',')
                             .Select(w => w.Trim())
                             .Where(w => !string.IsNullOrEmpty(w))
                             .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());

            return string.Join(", ", words);

        }

        [HttpGet]
        public IActionResult Start()
        {
            _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);
            _sessionHelper.SaveToSession<bool>(HttpContext, SessionKeys.IsEditOrganisationDetailsJourneySessionKey, false);
            return RedirectToAction("OrganisationType");
        }

        [HttpGet]
        public async Task<IActionResult> OrganisationType()
        {
            var model = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey) ?? new OrganisationModel();
            model.OrganisationTypes = OrganisationHelper.GetOrganisationTypeOptions();

            //check if the org id is present in session if so bind the data in OrganisationModel
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);
            if (orgId != null)
            {
                var org = await _organisationService.GetOrganisationById(orgId);

                model.SelectedOrganisationType = org?.Type.ToString();
                model.CompanyNumber = org?.CompaniesHouseNumber;
                model.CompanyDetails = new CompanyDetailsModel
                {
                    Title = org?.Name,
                    RegisteredOfficeAddress = new RegisteredOfficeAddressModel
                    {
                        AddressLine1 = org.RegisteredAddress?.AddressLine1,
                        AddressLine2 = org.RegisteredAddress?.AddressLine2,
                        Locality = org.RegisteredAddress?.Town,
                        Country = org.RegisteredAddress?.Country,
                        PostalCode = org.RegisteredAddress?.Postcode
                    }
                };

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, model);
            }


            bool isCheckAnswerFlow = _sessionHelper.GetIsCheckAnswerFlow(HttpContext);
            bool isEditOrganisationFlow = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.IsEditOrganisationDetailsJourneySessionKey);
            if (!isCheckAnswerFlow && !isEditOrganisationFlow)
            {
                this.ShowBackButton("Index", "Home");
            }
            if (isEditOrganisationFlow)
            {
                this.ShowBackButton("OrganisationDetails", "Dashboard");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OrganisationType(OrganisationModel model)
        {
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            if (orgModel != null)
            {
                model.CompanyNumber = orgModel.CompanyNumber;
                model.CompanyDetails = orgModel.CompanyDetails;
            }

            ModelState.Remove(nameof(model.CompanyNumber));
            ModelState.Remove(nameof(model.CompanyDetails));

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(orgModel?.SelectedOrganisationType) && orgModel.SelectedOrganisationType != model.SelectedOrganisationType)
                {
                    //reset model when the SelectedOrganisationType changed
                    model = new OrganisationModel()
                    {
                        SelectedOrganisationType = model.SelectedOrganisationType
                    };
                }

                string? selectedOrganisationTypeText = OrganisationHelper.GetOrganisationTypeOptions()
                    .FirstOrDefault(item => item.Value == model.SelectedOrganisationType)?.Text;

                if (selectedOrganisationTypeText == null)
                {
                    ModelState.AddModelError(nameof(model.SelectedOrganisationType), "Please select a valid organisation type.");
                    model.OrganisationTypes = OrganisationHelper.GetOrganisationTypeOptions();
                    return View("OrganisationType", model);
                }

                model.SelectedOrganisationTypeText = selectedOrganisationTypeText;

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, model);
                _sessionHelper.SaveToSession<bool>(HttpContext, "IsOverseasOrganisation", false);

                if (model.SelectedOrganisationType == Models.Enums.OrganisationType.OtherUkOrganisation.ToString())
                {
                    return RedirectToAction("OrganisationName");
                }
                else if (model.SelectedOrganisationType == Models.Enums.OrganisationType.OverseasOrganisation.ToString())
                {
                    _sessionHelper.SaveToSession<bool>(HttpContext, "IsOverseasOrganisation", true);
                    return RedirectToAction("OrganisationName");
                }

                return RedirectToAction("CompanyNumber");
            }

            model.OrganisationTypes = OrganisationHelper.GetOrganisationTypeOptions();
            return View("OrganisationType", model);
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult CompanyNumber()
        {
            this.ShowBackButton("OrganisationType");
            var model = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            return View("CompanyNumber", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public async Task<IActionResult> CompanyNumberAsync(OrganisationModel model)
        {
            this.ShowBackButton("OrganisationType");
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);

            orgModel.CompanyNumber = model.CompanyNumber;
            ModelState.Clear();
            TryValidateModel(orgModel);
            ModelState.Remove(nameof(orgModel.SelectedOrganisationType));

            CompanyDetailsModel? companyDetails = null;

            if (ModelState.IsValid)
            {
                try
                {
                    companyDetails = await _companiesHouseService.GetCompanyByNumberAsync(orgModel.CompanyNumber);
                    if (companyDetails == null)
                        ModelState.AddModelError(nameof(orgModel.CompanyNumber), "Company number not found. Please check and try again.");
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(nameof(orgModel.CompanyNumber), "Could not verify company number at this time. Please try again later.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(orgModel.CompanyNumber), "An unexpected error occurred during company number verification.");
                }
            }

            if (ModelState.IsValid && companyDetails != null)
            {
                orgModel.CompanyDetails = companyDetails;
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, orgModel);
                return RedirectToAction("CompanyConfirm");
            }
            return View("CompanyNumber", orgModel);
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult CompanyConfirm()
        {
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            if (organisationModel?.CompanyDetails == null)
                return RedirectToAction("CompanyNumber");
            if (!string.IsNullOrEmpty(organisationModel.CompanyNumber)) { 
                this.ShowBackButton("CompanyNumber");
            }
            else
            {
                this.ShowBackButton("OrganisationAddress");
            }
            return View("CompanyConfirm", organisationModel.CompanyDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public async Task<IActionResult> ConfirmAndContinue()
        {            
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);

            if (organisationModel?.CompanyDetails == null)
                return RedirectToAction("CompanyNumber");

            //Set empty contact details to ensure they pass the  [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))] action filter validation 
            //on user controller actions
            if (!string.IsNullOrEmpty(organisationModel?.CompanyNumber))
            {
                bool? alreadyExists = await _userService.IsOrganisationExists(organisationModel?.CompanyNumber);
                if (alreadyExists.HasValue && alreadyExists.Value)
                {
                    return RedirectToAction("AlreadyRegistered");
                }
            }
            else
            {
                var orgExists = await _organisationService.GetOrganisationByDetails(organisationModel.CompanyDetails.Title.Trim(),
                    organisationModel.CompanyDetails.RegisteredOfficeAddress.PostalCode.Trim(),
                    organisationModel.CompanyDetails.RegisteredOfficeAddress.Country.Trim());
                if (orgExists.HasValue && orgExists.Value == true)
                {
                    return RedirectToAction("AlreadyRegistered");
                }
            }

            var existingUserModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);

            if (existingUserModel == null)
            {
                existingUserModel = new UserModel();
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, existingUserModel);
            }

            var IsEditJourney = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.IsEditOrganisationDetailsJourneySessionKey);
            var IsAddNonRPOrgJourney = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.IsAddOrganisationDetailsNonRPJourneySessionKey);
            if (IsEditJourney || IsAddNonRPOrgJourney)
            {
                return RedirectToAction("UpdateOrganisationDetailsConfirmation");
            }

            return RedirectToAction("ConfirmResponsibility");
        }

        public IActionResult AlreadyRegistered()
        {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyConfirm");
            return View();
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult ConfirmResponsibility()
        {
            this.ShowBackButton("CompanyConfirm");
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            userModel.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;
            return View(userModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public IActionResult ConfirmResponsibility(UserModel model)
        {
            this.ShowBackButton("CompanyConfirm");
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            model.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;
            foreach (var field in new[] { "EmailAddress", "FirstName", "LastName", "PreferredContactType", "JobTitle" })
            {
                ModelState.Remove($"{nameof(model.ContactDetails)}.{field}");                
            }
            if (!ModelState.IsValid)
            {                
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, model);
            if (model.IsRegulatoryContact == true)
            {                
                return RedirectToAction("DeedPoll");
            }
            return RedirectToAction("CannotContinue");
        }        

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult DeedPoll()
        {
            this.ShowBackButton("ConfirmResponsibility");
            var model = _sessionHelper.GetFromSession<DeedPollViewModel>(HttpContext, "DeedPollViewModel") ?? new DeedPollViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public IActionResult DeedPoll(DeedPollViewModel model)
        {
            this.ShowBackButton("ConfirmResponsibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<DeedPollViewModel>(HttpContext, "DeedPollViewModel", model);
            if (model.IsDeedPollAccepted == true)
            {                
                var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey) ?? new UserModel();
                foreach (var field in new[] { "EmailAddress", "FirstName", "LastName", "PreferredContactType", "JobTitle" })
                {
                    ModelState.Remove($"{nameof(userModel.ContactDetails)}.{field}");
                }                
                if (string.IsNullOrEmpty(userModel.ContactDetails.EmailAddress))
                {
                    userModel.ContactDetails.EmailAddress = User.FindFirstValue("email");
                }
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, userModel);
                return RedirectToAction("ContactDetails");
            }
            return RedirectToAction("CannotContinue");
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult CannotContinue()
        {
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            if (userModel.IsRegulatoryContact == false)
            {
                this.ShowBackButton("ConfirmResponsibility");
            }
            else
            {
                this.ShowBackButton("DeedPoll");
            }

                ViewBag.OrganisationName = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey)?.CompanyDetails?.Title;
            return View("CannotContinue");
        }        

        [HttpGet]
        //[ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult ContactDetails()
        {
            var previousStep = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.PreviousStepKey);
            if(previousStep == "YourDetails")
            {
                this.ShowBackButton("Dashboard", "YourDetails");
            }
            else
            {
                this.ShowBackButton("DeedPoll");
            }                
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            var model = _sessionHelper.GetFromSession<OrganisationContactDetailsModel>(HttpContext, SessionKeys.OrganisationContactDetailsModelSessionKey) ?? userModel.ContactDetails;           
            return View(model);
        }

        // not in use
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public IActionResult SaveContactDetails(OrganisationContactDetailsModel contactDetails)
        {
            this.ShowBackButton("DeedPoll");
                       
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);

            if (string.IsNullOrEmpty(contactDetails.EmailAddress))
                contactDetails.EmailAddress = userModel.ContactDetails?.EmailAddress;

            switch (contactDetails.PreferredContactType)
            {
                case PreferredContactType.Landline:
                    contactDetails.MobileNumber = null;
                    ModelState.Remove(nameof(contactDetails.MobileNumber));
                    if (string.IsNullOrWhiteSpace(contactDetails.LandlineNumber))
                        ModelState.AddModelError(nameof(contactDetails.LandlineNumber), "Enter your landline number.");
                    break;
                case PreferredContactType.Mobile:
                    contactDetails.LandlineNumber = null;
                    contactDetails.ContactNumberExtension = null;
                    ModelState.Remove(nameof(contactDetails.LandlineNumber));
                    ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                    if (string.IsNullOrWhiteSpace(contactDetails.MobileNumber))
                        ModelState.AddModelError(nameof(contactDetails.MobileNumber), "Enter your mobile number.");
                    break;
                default:
                    contactDetails.LandlineNumber = null;
                    contactDetails.ContactNumberExtension = null;
                    contactDetails.MobileNumber = null;
                    ModelState.Remove(nameof(contactDetails.LandlineNumber));
                    ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                    ModelState.Remove(nameof(contactDetails.MobileNumber));
                    break;
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorSummary"] = "CustomErrorSummary";
                ViewBag.IsRegulatoryContact = true;
                return View("ContactDetails", contactDetails);
            }

            userModel.ContactDetails = contactDetails;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, userModel);

            return RedirectToAction("CheckYourAnswers");
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult CheckYourAnswers()
        {
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);

            var viewModel = new CheckYourAnswersModel
            {
                Organisation = organisationModel,
                User = userModel,
                ConfirmDeclaration = false
            };            

            // Set the flow state to Check Answers mode
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, true);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public async Task<IActionResult> SubmitAnswers(CheckYourAnswersModel viewModel)
        {
            viewModel.Organisation = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            viewModel.User = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);

            ModelState.Remove(nameof(viewModel.Organisation));
            ModelState.Remove(nameof(viewModel.User));

            if (!ModelState.IsValid)
            {
                ViewBag.ShowBackButton = false;
                return View("CheckYourAnswers", viewModel);
            }
            var organisationModel = viewModel.Organisation;
            var userModel = viewModel.User;

            var emailAddress = userModel?.ContactDetails?.EmailAddress;
            var company = organisationModel?.CompanyDetails;

            TempData["Confirmation_CompanyName"] = company?.Title;
            TempData["Confirmation_EmailAddress"] = emailAddress;

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            var regAddress = new RegisteredAddress(
                addressLine1: company?.RegisteredOfficeAddress?.AddressLine1,
                addressLine2: company?.RegisteredOfficeAddress?.AddressLine2,
                town: company?.RegisteredOfficeAddress?.Locality,
                postcode: company?.RegisteredOfficeAddress?.PostalCode,
                country: company?.RegisteredOfficeAddress?.Country);

            var preferredContactType = userModel?.ContactDetails?.PreferredContactType.ToApiModelType();
            var orgType = (OrganisationType)Enum.Parse(typeof(Models.Enums.OrganisationType), organisationModel.SelectedOrganisationType);

            var OrgId = string.Empty;

            try
            {

                var updateModel = new UpdateUserOrganisationRequest(
                    firstName: userModel?.ContactDetails?.FirstName,
                    lastName: userModel?.ContactDetails?.LastName,
                    preferredContactType: preferredContactType,
                    jobTitle: userModel?.ContactDetails?.JobTitle,
                    role: UserRole.ResponsiblePerson,
                    organisation: new OrganisationRequest
                    (
                        name: company?.Title,
                        type: orgType,
                        companiesHouseNumber: organisationModel.CompanyNumber,
                        registeredAddress: new RegisteredAddress2
                        (
                            addressLine1: regAddress.AddressLine1,
                            addressLine2: regAddress.AddressLine2,
                            town: regAddress.Town,
                            postcode: regAddress.Postcode,
                            country: regAddress.Country
                        )
                    ),
                    landlineNumber: userModel.ContactDetails.LandlineNumber,
                    contactNumberExtension: userModel.ContactDetails.ContactNumberExtension,
                    mobileNumber: userModel.ContactDetails.MobileNumber);


                OrgId = await _userService.UpdateUserOrganisation(userId, updateModel);

                TempData["Confirmation_Organisation_Id"] = OrgId;
                _logger.LogInformation("Successfully updated OrgDetails for user {UserId}. Retrieved OrgId: {OrgId}", userId, OrgId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubmitAnswers: An unexpected error occurred during API call for user {UserId}.", userId);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again or contact support.");
                ViewBag.ShowBackButton = false;
                return View("CheckYourAnswers", viewModel);
            }

            _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);

            return RedirectToAction("Confirmation");
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            var companyName = TempData["Confirmation_CompanyName"] as string;
            var emailAddress = TempData["Confirmation_EmailAddress"] as string;
            var orgId = TempData["Confirmation_Organisation_Id"] as string;


            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(orgId))
            {
                // Ensure any lingering session data is cleared before redirecting for a clean start.
                _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
                return RedirectToAction("Start", "Organisation"); // Redirect to the very beginning of the flow
            }

            //required for adding heat networks after org creation
            _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.OrganisationId, orgId);

            ViewBag.CompanyName = companyName;
            ViewBag.EmailAddress = emailAddress;
            ViewBag.OrganisationId = orgId;

            ViewBag.ShowBackButton = false;

            return View("Confirmation");
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult OrganisationName()
        {
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var model = new OtherOrganisationNameModel();
            if (organisationModel.SelectedOrganisationType == Models.Enums.OrganisationType.OtherUkOrganisation.ToString() ||
                organisationModel.SelectedOrganisationType == Models.Enums.OrganisationType.OverseasOrganisation.ToString())
            {
                model.OrganisationName = organisationModel.CompanyDetails?.Title;
            }
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("OrganisationType");

            return View("OrganisationName", model);
        }

        [HttpPost]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public IActionResult SaveOrganisationName(OtherOrganisationNameModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = Url.Action("OrganisationType");

                return View("OrganisationName", model);
            }

            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            if (organisationModel?.CompanyDetails == null)
            {
                organisationModel.CompanyDetails = new CompanyDetailsModel
                {
                    Title = model.OrganisationName.Trim()
                };
            }
            else
            {
                organisationModel.CompanyDetails.Title = model.OrganisationName;
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, organisationModel);

            return RedirectToAction("OrganisationAddress");
        }

        [HttpGet]
        public async Task<IActionResult> OrganisationAddressAsync()
        {
            this.ShowBackButton("OrganisationName", "Organisation");
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var viewModel = new AddressByStreetOrTownModel();
            if (organisationModel.CompanyDetails.RegisteredOfficeAddress != null)
            {
                viewModel = (AddressByStreetOrTownModel)organisationModel.CompanyDetails.RegisteredOfficeAddress;
            }
            var isOverseasOrganisation = _sessionHelper.GetFromSession<bool?>(HttpContext, "IsOverseasOrganisation") ?? false;
            ViewBag.IsOverseasOrganisation = isOverseasOrganisation;
            if (isOverseasOrganisation)
            {
                //convert country text to value to bind back
                var countries = await GetCountrySelectListItems();
                viewModel.Country = countries.FirstOrDefault(c => c.Text == viewModel.Country)?.Value ?? viewModel.Country;
                ViewBag.CountryList = countries;
            }
            return View("OrganisationAddress", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrganisationAddressAsync(AddressByStreetOrTownModel model)
        {
            var isOverseasOrganisation = _sessionHelper.GetFromSession<bool?>(HttpContext, "IsOverseasOrganisation") ?? false;

            var countries = new List<SelectListItem>();

            if (isOverseasOrganisation)
            {
                countries = await GetCountrySelectListItems();

                if (countries.FirstOrDefault(c => c.Value == model.Country) == null)
                {
                    //add model error
                    ModelState.AddModelError(nameof(model.Country), "Please select a valid country.");
                }
                else
                {
                    model.Country = countries.FirstOrDefault(c => c.Value == model.Country)?.Text ?? model.Country;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.Postalcode) &&
                               !Regex.IsMatch(model.Postalcode.Trim().ToUpper(), "^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|[A-HK-Y][0-9]{1,2}|[0-9][A-HJKS-UW]|[A-HK-Y][0-9][ABEHMNPRV-Y]) ?[0-9][ABD-HJLNP-UW-Z]{2})$"))
                {
                    ModelState.AddModelError(nameof(model.Postalcode), "Please enter a valid UK postcode.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.IsOverseasOrganisation = isOverseasOrganisation;

                if (isOverseasOrganisation)
                {
                    ViewBag.CountryList = countries;
                }

                this.ShowBackButton("OrganisationName");
                // Return the view with the model to preserve user input and show errors
                return View("OrganisationAddress", model);
            }

            // Join non-empty fields with commas
            var addressParts = new[] { model.StreetAddress, model.TownOrCity, model.Postalcode, model.Country }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            model.Fulladdress = string.Join(", ", addressParts);

            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            organisationModel.CompanyDetails.RegisteredOfficeAddress = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, organisationModel);

            return RedirectToAction("CompanyConfirm");
        }

        [HttpGet]
        public IActionResult OrganisationAddressByPostcode()
        {
            this.ShowBackButton("OrganisationName", "Organisation");
            ModelState.Clear();
            var model = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey) ?? new SearchAddressByPostcodeModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> OrganisationAddressByPostcode(SearchAddressByPostcodeModel model)
        {
            this.ShowBackButton("OrganisationName", "Organisation");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var results = await _addressLookUpService.PostcodeLookupAsync(model.Postcode);
                model.Postcode = model.Postcode?.ToUpperInvariant().Trim();
                if (results == null || results.Addresses == null || results.Addresses.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode.");
                    return View(model);
                }
                results.Addresses = results.Addresses
                    .Select(address => CapitalizeCommaSeparated(address))
                    .ToArray();

                _sessionHelper.SaveToSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey, results);
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.PreviousStepKey, "Organisation");
                return RedirectToAction("SearchByPostcodeResults", "Address");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Unable to retrieve address data.");
                return View(model);
            }
        }        

        public async Task<List<SelectListItem>> GetCountrySelectListItems()
        {
            var countryAndTerritories = await _countriesAndTerritoriesService.GetCountriesAndTerritories();

            if (countryAndTerritories == null || !countryAndTerritories.Any())
            {
                return new List<SelectListItem>();
            }

            // 3. Project and sort the data
            var countryListItems = countryAndTerritories
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.FullValue
                })
                .OrderBy(c => c.Text)
                .ToList();

            return countryListItems;
        }

        [HttpGet]
        public async Task<IActionResult> UpdateOrganisationDetailsConfirmation()
        {
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var IsAddNonRPOrgJourney = _sessionHelper.GetFromSession<bool>(HttpContext, SessionKeys.IsAddOrganisationDetailsNonRPJourneySessionKey);

            if (organisationModel?.CompanyDetails == null)
            {
                _logger.LogWarning("UpdateOrganisationDetailsConfirmation: Missing session data (organisation or user).");
                // Clear any flow data to ensure clean state and redirect to start of flow
                _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
                _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);
                _sessionHelper.SaveToSession<bool>(HttpContext, SessionKeys.IsEditOrganisationDetailsJourneySessionKey, false);
                return BadRequest("Required session data is missing.");
            }

            // Build registered address
            CompanyDetailsModel company = organisationModel.CompanyDetails;
            RegisteredOfficeAddressModel regAddrModel = company.RegisteredOfficeAddress;

            var regAddress = new RegisteredAddress(
                addressLine1: regAddrModel?.AddressLine1?.Trim(),
                addressLine2: regAddrModel.AddressLine2?.Trim(),
                town: regAddrModel?.Locality?.Trim(),
                postcode: regAddrModel?.PostalCode?.Trim(),
                country: regAddrModel?.Country?.Trim()
            );


            // Map organisation type (safe parse)
            OrganisationType apiOrgType;
            try
            {
                apiOrgType = (OrganisationType)Enum.Parse(typeof(Models.Enums.OrganisationType), organisationModel.SelectedOrganisationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateOrganisationDetailsConfirmation: Failed to parse organisation type '{OrgType}'", organisationModel.SelectedOrganisationType);
                ModelState.AddModelError(string.Empty, "Unable to determine organisation type.");
                return View();
            }

            // Build organisation request
            var orgRequest = new OrganisationRequest(
                name: company.Title,
                type: apiOrgType,
                companiesHouseNumber: organisationModel.CompanyNumber,
                registeredAddress: new RegisteredAddress2
                (
                    addressLine1: regAddress.AddressLine1,
                    addressLine2: regAddress.AddressLine2,
                    town: regAddress.Town,
                    postcode: regAddress.Postcode,
                    country: regAddress.Country
                )
            );

            try
            {
                if (IsAddNonRPOrgJourney)
                {
                    //Save the organisation details in session and redirect to add contact details for non RP user
                    var response = await _userService.UpdateOrganisationLinkUser(userId, orgRequest);
                    _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);

                    ViewBag.OrganisationId = response?.OrgId;
                    ViewBag.ShowBackButton = false;
                    return View("Organisation/Confirmation");
                }


                var userDetails = await _userService.GetUserDetails(userId);
                var orgId = userDetails?.Organisation?.OrgId;


                await _organisationService.EditOrganisationDetails(orgId, orgRequest, userId);

                _logger.LogInformation("UpdateOrganisationDetailsConfirmation: Successfully updated organisation for user {UserId}. OrgId: {OrgId}", userId, orgId);

                // Clear flow session data and reset flags
                _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
                _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);
                _sessionHelper.SaveToSession<bool>(HttpContext, SessionKeys.IsEditOrganisationDetailsJourneySessionKey, false);
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateOrganisationDetailsConfirmation: An unexpected error occurred while updating organisation for user {UserId}.", userId);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating organisation details. Please try again later.");
                return View();
            }
        }
    }
}