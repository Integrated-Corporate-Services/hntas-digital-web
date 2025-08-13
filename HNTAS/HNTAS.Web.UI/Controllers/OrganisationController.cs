using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using PreferredContactType = HNTAS.Web.UI.Models.PreferredContactType;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class OrganisationController : Controller
    {
        private readonly ICompaniesHouseService _companiesHouseService;
        private readonly ILogger<OrganisationController> _logger;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;

        public OrganisationController(ICompaniesHouseService companiesHouseService, ILogger<OrganisationController> logger, IUserService userService, ISessionHelper sessionHelper)
        {
            _companiesHouseService = companiesHouseService;
            _logger = logger;
            _userService = userService;
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        public IActionResult Start()
        {
            _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);
            return RedirectToAction("OrganisationType");
        }

        [HttpGet]
        public IActionResult OrganisationType()
        {
            var model = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey) ?? new OrganisationModel();
            model.OrganisationTypes = GetOrganisationTypeOptions();

            bool isCheckAnswerFlow = _sessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = isCheckAnswerFlow ? false : true;
            ViewBag.BackLinkUrl = Url.Action("Index", "Home");

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

                string? selectedOrganisationTypeText = GetOrganisationTypeOptions()
                    .FirstOrDefault(item => item.Value == model.SelectedOrganisationType)?.Text;

                if (selectedOrganisationTypeText == null)
                {
                    ModelState.AddModelError(nameof(model.SelectedOrganisationType), "Please select a valid organisation type.");
                    model.OrganisationTypes = GetOrganisationTypeOptions();
                    return View("OrganisationType", model);
                }

                model.SelectedOrganisationTypeText = selectedOrganisationTypeText;

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, model);

                if (model.SelectedOrganisationType == Models.OrganisationType.OtherUkOrganisation.ToString() || model.SelectedOrganisationType == Models.OrganisationType.OverseasOrganisation.ToString())
                {
                    return RedirectToAction("OrganisationName");
                }

                return RedirectToAction("CompanyNumber");
            }

            model.OrganisationTypes = GetOrganisationTypeOptions();
            return View("OrganisationType", model);
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult CompanyNumber()
        {
            var model = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("OrganisationType");

            return View("CompanyNumber", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public async Task<IActionResult> CompanyNumberAsync(OrganisationModel model)
        {
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

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("OrganisationType");

            return View("CompanyNumber", orgModel);
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult CompanyConfirm()
        {
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);

            if (organisationModel?.CompanyDetails == null)
                return RedirectToAction("CompanyNumber");

            ViewBag.ShowBackButton = true;
            string backPageUrl = !string.IsNullOrEmpty(organisationModel.CompanyNumber) ? Url.Action("CompanyNumber") : Url.Action("OrganisationAddress");

            ViewBag.BackLinkUrl = ViewBag.ChangeUrl = backPageUrl;

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

            var existingUserModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);

            if (existingUserModel == null)
            {
                existingUserModel = new UserModel();
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, existingUserModel);
            }

            return RedirectToAction("ConfirmRegulatoryContact");
        }

        public IActionResult AlreadyRegistered()
        {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyConfirm");
            return View();
        }


        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult ConfirmRegulatoryContact()
        {
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);

            userModel.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyConfirm");

            return View(userModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public IActionResult ConfirmRegulatoryContact(UserModel model)
        {
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);

            foreach (var field in new[] { "EmailAddress", "FirstName", "LastName", "PreferredContactType", "JobTitle" })
            {
                ModelState.Remove($"{nameof(model.ContactDetails)}.{field}");
            }

            if (!ModelState.IsValid)
            {
                model.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;

                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = Url.Action("CompanyConfirm");

                return View(model);
            }

            var sessionUserModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey) ?? new UserModel();

            if (sessionUserModel != null)
            {
                model.ContactDetails = sessionUserModel.ContactDetails;
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, model);

            if (model.IsRegulatoryContact == true)
            {
                if (string.IsNullOrEmpty(model.ContactDetails.EmailAddress))
                {
                    model.ContactDetails.EmailAddress = User.FindFirstValue("email");
                    _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, model);
                }

                return RedirectToAction("ContactDetails");
            }
            return RedirectToAction("CannotContinue");
        }



        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult CannotContinue()
        {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("ConfirmRegulatoryContact");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey)?.CompanyDetails?.Title;
            return View("CannotContinue");
        }

        [HttpGet]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnGetAttribute))]
        public IActionResult ContactDetails()
        {
            var orgModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("ConfirmRegulatoryContact");

            return View(userModel.ContactDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(EnsureSessionForOrganisationFlowOnPostAttribute))]
        public IActionResult SaveContactDetails(ContactDetailsModel contactDetails)
        {
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
                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = Url.Action("ConfirmRegulatoryContact");
                TempData["ErrorSummary"] = "CustomErrorSummary";

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
                ConfirmedDeclaration = false
            };

            ViewBag.ShowBackButton = false;

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

            var regAddress = new OrgRegisteredAddress(
                addressLine1: company?.RegisteredOfficeAddress?.AddressLine1,
                addressLine2: company?.RegisteredOfficeAddress?.AddressLine2,
                town: company?.RegisteredOfficeAddress?.Locality,
                postcode: company?.RegisteredOfficeAddress?.PostalCode,
                country: company?.RegisteredOfficeAddress?.Country);

            var preferredContactType = userModel?.ContactDetails?.PreferredContactType == PreferredContactType.Landline ? HNTAS.Api.Client.Model.PreferredContactType.Landline : HNTAS.Api.Client.Model.PreferredContactType.Mobile;
            var orgType = (Api.Client.Model.OrganisationType)Enum.Parse(typeof(Models.OrganisationType), organisationModel.SelectedOrganisationType);


            try
            {

                var OrgId = await _userService.UpdateUserOrganisation(userId, new UpdateOrgDetailsAndRolesRequest(new OrgDetails2(
                    orgType: orgType,
                    companiesHouseNumber: organisationModel.CompanyNumber,
                    orgName: company?.Title,
                    firstName: userModel?.ContactDetails?.FirstName,
                    lastName: userModel?.ContactDetails?.LastName,
                    preferredContactType: preferredContactType,//(HNTAS.Api.Client.Model.PreferredContactType)(int)userModel?.ContactDetails?.PreferredContactType,
                    orgRegisteredAddress: regAddress,
                    orgId: null,
                    landlineNumber: userModel?.ContactDetails?.LandlineNumber,
                    contactNumberExtension: userModel?.ContactDetails?.ContactNumberExtension,
                    mobileNumber: userModel?.ContactDetails?.MobileNumber,
                    jobTitle: userModel?.ContactDetails?.JobTitle), UserRole.RegulatoryContact));

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
            if (organisationModel.SelectedOrganisationType == Models.OrganisationType.OtherUkOrganisation.ToString() ||
                organisationModel.SelectedOrganisationType == Models.OrganisationType.OverseasOrganisation.ToString())
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
                    Title = model.OrganisationName
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
        public IActionResult OrganisationAddress()
        {
            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            var model = new ManualOfficeAddressModel();

            if (organisationModel.SelectedOrganisationType == Models.OrganisationType.OtherUkOrganisation.ToString() ||
                organisationModel.SelectedOrganisationType == Models.OrganisationType.OverseasOrganisation.ToString())
            {
                if (organisationModel.CompanyDetails?.RegisteredOfficeAddress is RegisteredOfficeAddressModel address)
                {
                    model = new ManualOfficeAddressModel
                    {
                        AddressLine1 = address.AddressLine1,
                        AddressLine2 = address.AddressLine2,
                        Locality = address.Locality,
                        PostalCode = address.PostalCode,
                        Country = address.Country
                    };
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("OrganisationName");

            return View("OrganisationAddress", model);
        }

        [HttpPost]
        public IActionResult SaveOrganisationAddress(ManualOfficeAddressModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = Url.Action("OrganisationName");

                return View("OrganisationAddress", model);
            }

            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey);
            organisationModel.CompanyDetails.RegisteredOfficeAddress = model;

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationCreation_SessionKey, organisationModel);

            return RedirectToAction("CompanyConfirm");
        }

        private static List<SelectListItem> GetOrganisationTypeOptions()
        {
            return
            [
                new SelectListItem { Value = Models.OrganisationType.UkCompaniesHouse.ToString(), Text = "UK company registered with Companies House" },
                new SelectListItem { Value = Models.OrganisationType.OtherUkOrganisation.ToString(), Text = "Other UK organisation" },
                new SelectListItem { Value = Models.OrganisationType.OverseasOrganisation.ToString(), Text = "Overseas organisation" }
            ];
        }
    }
}