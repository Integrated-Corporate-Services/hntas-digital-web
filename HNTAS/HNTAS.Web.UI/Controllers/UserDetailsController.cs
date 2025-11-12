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
        public UserDetailsController(ISessionHelper sessionHelper, IUserService userService)
        {
            _sessionHelper = sessionHelper;
            _userService = userService;
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

            return RedirectToAction("CheckYourAnswers");
        }

        [HttpGet]
        public IActionResult CheckYourAnswers() 
        {
            this.ShowBackButton("ContactDetails", "UserDetails");
            var userModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
            var viewModel = new CheckYourAnswersModel
            {
                Organisation = null,
                User = userModel,
                ConfirmDeclaration = false
            };
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, true);
            return View("UserDetails/CheckYourAnswers", viewModel);
        }

        [HttpPost]
        public IActionResult SubmitAnswers()
        {
            // and the api call tp save it 
            return RedirectToAction("ManageUsers", "UserManagement");
        }

    }
}
