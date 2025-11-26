using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using PreferredContactType = HNTAS.Web.UI.Models.Enums.PreferredContactType;


namespace HNTAS.Web.UI.Controllers
{
    public class UserDetailsController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IUserService _userService;
        private readonly ILogger<UserDetailsController> _logger = null;
        public UserDetailsController(ISessionHelper sessionHelper, IUserService userService, ILogger<UserDetailsController> logger)
        {
            _sessionHelper = sessionHelper;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ContactDetails()
        {
            this.ShowBackButton("ManageUsers", "UserManagement");
            var isAssessorOrCertifier = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.IsAssessorOrCertifier);
            if (isAssessorOrCertifier == "true")
            {
                this.ShowBackButton("Assessor", "UserDetails");
            }
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            if (userModel == null || userModel.ContactDetails == null || userModel.ContactDetails.JobTitle == null)
            {
                var userDetails = await _userService.GetUserDetails(userId);
                var isUserRp = await _userService.IsRpUserAsync(userDetails.EmailId);

                PreferredContactType? preferredContactType = userDetails?.PreferredContactType.ToViewModelType();

                userModel = new UserModel
                {
                    IsRegulatoryContact = isUserRp,
                    OrganisationName = userDetails.Organisation?.Name,
                    ContactDetails = {
                        FirstName = userDetails.FirstName,
                        LastName = userDetails.LastName,
                        PreferredContactType = preferredContactType,
                        EmailAddress = userDetails.EmailId,
                        JobTitle = userDetails.JobTitle,
                        LandlineNumber = userDetails.LandlineNumber,
                        MobileNumber = userDetails.MobileNumber,
                        ContactNumberExtension = userDetails.ContactNumberExtension
                    }
                };
            }

            _sessionHelper.SaveToSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey, userModel);
            ViewBag.NextActionController = "UserDetails";
            ViewBag.IsRegulatoryContact = userModel.IsRegulatoryContact;
            return View("UserDetails/ContactDetails", userModel.ContactDetails);
        }

        [HttpPost]
        public async Task<IActionResult> SaveContactDetails(OrganisationContactDetailsModel contactDetails)
        {
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            var isUserRP = await _userService.IsRpUserAsync(userModel.ContactDetails.EmailAddress) ?? false;

            if (string.IsNullOrEmpty(contactDetails.EmailAddress))
                contactDetails.EmailAddress = userModel.ContactDetails?.EmailAddress;

            if (contactDetails.PreferredContactType == null)
            {
                //add model error if preferred contact type is not selected
                TempData["ErrorSummary"] = "CustomErrorSummary";
                ModelState.AddModelError(nameof(contactDetails.PreferredContactType), "Select your preferred contact method.");
                return View("UserDetails/ContactDetails", contactDetails);
            }

            //if (isUserRP)
            {
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
                    case PreferredContactType.PreferNotToSay:
                        contactDetails.LandlineNumber = null;
                        contactDetails.ContactNumberExtension = null;
                        contactDetails.MobileNumber = null;
                        ModelState.Remove(nameof(contactDetails.LandlineNumber));
                        ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                        ModelState.Remove(nameof(contactDetails.MobileNumber));
                        break;
                }
            }


            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ManageUsers", "UserManagement");
                TempData["ErrorSummary"] = "CustomErrorSummary";

                return View("UserDetails/ContactDetails", contactDetails);
            }

            userModel.ContactDetails = contactDetails;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, userModel);

            return RedirectToAction("CheckYourAnswers", "UserDetails");
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            this.ShowBackButton("ContactDetails", "UserDetails");
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            var viewModel = new CheckYourAnswersModel
            {
                Organisation = new OrganisationModel(),
                User = userModel,
                ConfirmDeclaration = true
            };
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, true);
            return View("UserDetails/CheckYourAnswers", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswers()
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            var userDetails = await _userService.GetUserDetails(userId);

            var preferredContactType = userModel?.ContactDetails?.PreferredContactType.ToApiModelType();

            try
            {
                var updateModel = new UpdateUserDetailsRequest(
                    firstName: userModel?.ContactDetails?.FirstName,
                    lastName: userModel?.ContactDetails?.LastName,
                    preferredContactType: preferredContactType,
                    jobTitle: userModel?.ContactDetails?.JobTitle,
                    landlineNumber: userModel.ContactDetails.LandlineNumber,
                    contactNumberExtension: userModel.ContactDetails.ContactNumberExtension,
                    mobileNumber: userModel.ContactDetails.MobileNumber);
                await _userService.UpdateUserDetails(userId, updateModel);

                _logger.LogInformation("Successfully updated User Details for {UserId}", userId);

                _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
                _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);
                return RedirectToAction("ManageUsers", "UserManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OrgDetails for user {UserId}", userId);
                TempData["ErrorMessage"] = "There was a problem saving your details. Please try again.";
                return View("UserDetails/CheckYourAnswers", new CheckYourAnswersModel
                {
                    Organisation = new OrganisationModel(),
                    User = userModel,
                    ConfirmDeclaration = true
                });
            }
        }
    }
}
