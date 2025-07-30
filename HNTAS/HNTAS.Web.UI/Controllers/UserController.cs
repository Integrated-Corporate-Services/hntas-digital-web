using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PreferredContactType = HNTAS.Web.UI.Models.PreferredContactType;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult ConfirmRPIsRC()
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            userModel.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = isCheckAnswerFlow
                ? Url.Action("CheckAnswers", "User")
                : Url.Action("CompanyConfirm", "Organisation");

            return View(userModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public IActionResult ConfirmRPIsRC(UserModel model)
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

                bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = isCheckAnswerFlow
                    ? Url.Action("CheckAnswers", "User")
                    : Url.Action("CompanyConfirm", "Organisation");

                return View("ConfirmRPIsRC", model);
            }

            var sessionUserModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();

            if(sessionUserModel != null)
            {
                model.ContactDetails = sessionUserModel.ContactDetails;
            }

            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, model);

            if (model.ifRPisRC == true)
            {
                if (string.IsNullOrEmpty(model.ContactDetails.EmailAddress))
                {
                    model.ContactDetails.EmailAddress = User.FindFirstValue("email");
                    SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, model);
                }

                bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
                if (isCheckAnswerFlow)
                    return RedirectToAction("CheckAnswers", "User");

                return RedirectToAction("ContactDetails");
            }
            return RedirectToAction("Guidance", "Guidance");
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult ContactDetails()
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = isCheckAnswerFlow
                ? Url.Action("CheckAnswers", "User")
                : Url.Action("ConfirmRPIsRC", "User");

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
                bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = isCheckAnswerFlow
                    ? Url.Action("CheckAnswers", "User")
                    : Url.Action("ConfirmRPIsRC", "User");

                return View("ContactDetails", contactDetails);
            }

            userModel.ContactDetails = contactDetails;
            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, userModel);

            return RedirectToAction("CheckAnswers");
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CheckAnswers()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);           

            var viewModel = new CheckAnswersModel
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
        public async Task<IActionResult> SubmitAnswers(CheckAnswersModel viewModel)
        {
            viewModel.Organisation = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            viewModel.User = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            ModelState.Remove(nameof(viewModel.Organisation));
            ModelState.Remove(nameof(viewModel.User));

            if (!ModelState.IsValid)
            {
                ViewBag.ShowBackButton = false;
                return View("CheckAnswers", viewModel);
            }
            var organisationModel = viewModel.Organisation;
            var userModel = viewModel.User;

            var emailAddress = userModel?.ContactDetails?.EmailAddress;
            var company = organisationModel?.CompanyDetails;

            TempData["Confirmation_CompanyName"] = company?.Title;
            TempData["Confirmation_EmailAddress"] = emailAddress;

            var userId = SessionHelper.GetFromSession<string>(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey);

            var regAddress = new OrgRegisteredAddress(
                addressLine1 : company?.RegisteredOfficeAddress?.AddressLine1,
                addressLine2: company?.RegisteredOfficeAddress?.AddressLine2,
                town: company?.RegisteredOfficeAddress?.Locality,
                postcode: company?.RegisteredOfficeAddress?.PostalCode,
                country: company?.RegisteredOfficeAddress?.Country);

            var preferredContactType = userModel?.ContactDetails?.PreferredContactType == PreferredContactType.Landline ? HNTAS.Api.Client.Model.PreferredContactType.Landline : HNTAS.Api.Client.Model.PreferredContactType.Mobile;

            

            try
            {

                var OrgId = await _userService.UpdateUserOrganisation(userId, new UpdateOrgDetailsAndRolesRequest(new OrgDetails2(
                    orgType: organisationModel.SelectedOrganisationTypeText,
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
                return View("CheckAnswers", viewModel);
            }

            SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, false);

            return RedirectToAction("Confirmation", "User");
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
        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var userId = SessionHelper.GetFromSession<string>(HttpContext, SessionHelper.SessionKeys.UserModel_Id_SessionKey);

                var user = await _userService.GetUserById(userId);

                var viewModel = new ManageUsersModel
                {
                    OrganisationName = user.Organisation?.Name,
                    Users = new List<UserDisplayModel> { new UserDisplayModel
                    {
                        Id = user.Id,
                        EmailAddress = user.EmailAddress,
                        Name = user.FullName,
                        Roles = user.Roles.Select(r => r.ToString()).ToList(),
                        Status = user.Status.ToString()
                    } }
                };

                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = Url.Action("UserAccount", "Dashboard");

                return View("ManageUsers", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to manage users.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return View("ManageUsers");
            }
        }
    }
}