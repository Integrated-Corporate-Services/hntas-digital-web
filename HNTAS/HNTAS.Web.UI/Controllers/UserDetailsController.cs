using HNTAS.Api.Client.Model;
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
        public IActionResult ContactDetails()
        {
            this.ShowBackButton("ManageUsers", "UserManagement");
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            ViewBag.NextActionController = "UserDetails";
            return View("UserDetails/ContactDetails" ,userModel.ContactDetails);
        }

        [HttpPost]
        public async Task<IActionResult> SaveContactDetails(OrganisationContactDetailsModel contactDetails)
        {
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            var isUserRP = await _userService.IsRpUserAsync(userModel.ContactDetails.EmailAddress) ?? false;

            if (string.IsNullOrEmpty(contactDetails.EmailAddress))
                contactDetails.EmailAddress = userModel.ContactDetails?.EmailAddress;
            if (isUserRP) 
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
                    default:
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
            
            var preferredContactType = userModel?.ContactDetails?.PreferredContactType == PreferredContactType.Landline ? HNTAS.Api.Client.Model.PreferredContactType.Landline : HNTAS.Api.Client.Model.PreferredContactType.Mobile;

            try 
            {
                var updateModel = new UpdateUserOrganisationRequest(
                    firstName: userModel?.ContactDetails?.FirstName,
                    lastName: userModel?.ContactDetails?.LastName,
                    preferredContactType: preferredContactType,
                    jobTitle: userModel?.ContactDetails?.JobTitle,
                    role: userDetails.Roles[0],
                    organisation: new OrganisationRequest
                    (
                        name: userDetails.Organisation.Name,
                        type: (OrganisationType)userDetails.Organisation.Type,
                        companiesHouseNumber: userDetails.Organisation.CompaniesHouseNumber,
                        registeredAddress: userDetails.Organisation.RegisteredAddress
                    ),
                    landlineNumber: userModel.ContactDetails.LandlineNumber,
                    contactNumberExtension: userModel.ContactDetails.ContactNumberExtension,
                    mobileNumber: userModel.ContactDetails.MobileNumber);
                var orgId = await _userService.UpdateUserOrganisation(userId, updateModel);
                _logger.LogInformation("Successfully updated OrgDetails for user {UserId}. Retrieved OrgId: {OrgId}", userId, orgId);
                return RedirectToAction("ManageUsers", "UserManagement");
            }
            catch(Exception ex)
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
