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

        public OrganisationController(ICompaniesHouseService companiesHouseService, ILogger<OrganisationController> logger, IUserService userService)
        {
            _companiesHouseService = companiesHouseService;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Start()
        {
            SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, false);
            return RedirectToAction("OrganisationType");
        }

        [HttpGet]
        public IActionResult OrganisationType()
        {
            var model = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey) ?? new OrganisationModel();
            model.OrganisationTypes = GetOrganisationTypeOptions();

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = isCheckAnswerFlow ? false : true;
            ViewBag.BackLinkUrl = Url.Action("Index", "Home");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Type(OrganisationModel model)
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            if (orgModel != null)
            {
                model.CompanyNumber = orgModel.CompanyNumber;
                model.CompanyDetails = orgModel.CompanyDetails;
            }

            ModelState.Remove(nameof(model.CompanyNumber));
            ModelState.Remove(nameof(model.CompanyDetails));

            if (ModelState.IsValid)
            {
                string? selectedOrganisationTypeText = GetOrganisationTypeOptions()
                    .FirstOrDefault(item => item.Value == model.SelectedOrganisationType)?.Text;

                if (selectedOrganisationTypeText == null)
                {
                    ModelState.AddModelError(nameof(model.SelectedOrganisationType), "Please select a valid organisation type.");
                    model.OrganisationTypes = GetOrganisationTypeOptions();
                    return View("OrganisationType", model);
                }

                model.SelectedOrganisationTypeText = selectedOrganisationTypeText;

                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey, model);

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
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CompanyNumber()
        {
            var model = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("OrganisationType");

            return View("CompanyNumber", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public async Task<IActionResult> CompanyNumberAsync(OrganisationModel model)
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

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
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey, orgModel);
                return RedirectToAction("CompanyConfirm");
            }

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("Type");

            return View("CompanyNumber", orgModel);
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CompanyConfirm()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            if (organisationModel?.CompanyDetails == null)
                return RedirectToAction("CompanyNumber");

            ViewBag.ShowBackButton = true;
            string backPageUrl = !string.IsNullOrEmpty(organisationModel.CompanyNumber) ? Url.Action("CompanyNumber") : Url.Action("OrganisationAddress");

            ViewBag.BackLinkUrl = ViewBag.ChangeUrl = backPageUrl;

            return View("CompanyConfirm", organisationModel.CompanyDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public async Task<IActionResult> ConfirmAndContinue()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            if (organisationModel?.CompanyDetails == null)
                return RedirectToAction("CompanyNumber");

            //Set empty contact details to ensure they pass the EnsureSessionForOrganisationFlowOnPost action filter validation 
            //on user controller actions
            if (!string.IsNullOrEmpty(organisationModel?.CompanyNumber))
            {
                bool? alreadyExists = await _userService.IsOrganisationExists(organisationModel?.CompanyNumber);
                if (alreadyExists.HasValue && alreadyExists.Value)
                {
                    return RedirectToAction("AlreadyRegistered");
                }
            }

            var existingUserModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            if (existingUserModel == null)
            {
                existingUserModel = new UserModel();
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, existingUserModel);
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
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult ConfirmRegulatoryContact()
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            userModel.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyConfirm");

            return View(userModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public IActionResult ConfirmRegulatoryContact(UserModel model)
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

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

            var sessionUserModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();

            if (sessionUserModel != null)
            {
                model.ContactDetails = sessionUserModel.ContactDetails;
            }

            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, model);

            if (model.IsRegulatoryContact == true)
            {
                if (string.IsNullOrEmpty(model.ContactDetails.EmailAddress))
                {
                    model.ContactDetails.EmailAddress = User.FindFirstValue("email");
                    SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, model);
                }

                return RedirectToAction("ContactDetails");
            }
            return RedirectToAction("CannotContinue");
        }



        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CannotContinue()
        {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("ConfirmRegulatoryContact");
            ViewBag.OrganisationName = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey)?.CompanyDetails?.Title;
            return View("CannotContinue");
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult ContactDetails()
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("ConfirmRegulatoryContact");

            return View(userModel.ContactDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public IActionResult SaveContactDetails(ContactDetailsModel contactDetails)
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

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
            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, userModel);

            return RedirectToAction("CheckYourAnswers");
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CheckYourAnswers()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            var viewModel = new CheckYourAnswersModel
            {
                Organisation = organisationModel,
                User = userModel,
                ConfirmedDeclaration = false
            };

            ViewBag.ShowBackButton = false;

            // Set the flow state to Check Answers mode
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, true);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public async Task<IActionResult> SubmitAnswers(CheckYourAnswersModel viewModel)
        {
            viewModel.Organisation = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            viewModel.User = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

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

            var userId = SessionHelper.GetFromSession<string>(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey);

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

            SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, false);

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
                SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
                return RedirectToAction("Start", "Organisation"); // Redirect to the very beginning of the flow
            }

            ViewBag.CompanyName = companyName;
            ViewBag.EmailAddress = emailAddress;
            ViewBag.OrganisationId = orgId;

            ViewBag.ShowBackButton = false;

            return View("Confirmation");
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult OrganisationName()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var model = new OtherOrganisationNameModel();
            if (organisationModel.SelectedOrganisationType == Models.OrganisationType.OtherUkOrganisation.ToString() ||
                organisationModel.SelectedOrganisationType == Models.OrganisationType.OverseasOrganisation.ToString())
            {
                model.OrganisationName = organisationModel.CompanyDetails?.Title;
            }
            return View("OrganisationName", model);
        }

        [HttpPost]
        [EnsureSessionForOrganisationFlowOnPost]
        public IActionResult SaveOrganisationName(OtherOrganisationNameModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("OrganisationName", model);
            }

            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
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

            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey, organisationModel);

            return RedirectToAction("OrganisationAddress");
        }

        [HttpGet]
        public IActionResult OrganisationAddress()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var model = new RegisteredOfficeAddressModel();

            if (organisationModel.SelectedOrganisationType == Models.OrganisationType.OtherUkOrganisation.ToString() ||
                organisationModel.SelectedOrganisationType == Models.OrganisationType.OverseasOrganisation.ToString())
            {
                model = organisationModel.CompanyDetails?.RegisteredOfficeAddress ?? model;
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View("OrganisationAddress", model);
        }

        [HttpPost]
        public IActionResult SaveOrganisationAddress(RegisteredOfficeAddressModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("OrganisationAddress", model);
            }

            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            organisationModel.CompanyDetails.RegisteredOfficeAddress = model;

            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey, organisationModel);

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