using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.Review;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;


namespace HNTAS.Web.UI.Controllers
{
    public class NewContributorController : Controller
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly ILogger<NewContributorController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly IHeatNetworksApi _heatNetworksApi;
        private readonly IInvitationTokenService _iInvitationTokenService;

        public NewContributorController(ILogger<NewContributorController> logger,
            IWorkflowManager workflowManager,
            ISessionHelper sessionHelper,
            IUserService userService,
            IHeatNetworksApi heatNetworksApi,
            IInvitationTokenService invitationTokenService,
            IInvitationService invitationService)
        {
            _logger = logger;
            _workflowManager = workflowManager;
            _sessionHelper = sessionHelper;
            _userService = userService;
            _heatNetworksApi = heatNetworksApi;
            _iInvitationTokenService = invitationTokenService;
            _invitationService = invitationService;
        }


        [HttpGet]
        public IActionResult AddEmailAddress()
        {
            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();

            this.ShowBackButton("AddContributor", "UserManagement");

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            return View(state.Data.AddUserEmailAddressModel ?? new AddUserEmailAddressModel());
        }

        private async Task<bool> DoesUserAlreadyExist(string newUserEmailID)
        {
            // Call API to get list of contributors
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var contributors = await _userService.GetRegisteredUsersAsync(userId);

            // Check for null or empty list and return an empty list if necessary
            if (contributors == null || !contributors.Any())
            {
                _logger.LogWarning("No contributors found for the current user ID {UserId}.", userId);
                return false;
            }

            // Map contributors to a list of SelectListItem
            var contributorEmailIds = contributors.Select(item => item.EmailId).ToList();

            bool isExistingUser = false;
            foreach (var email in contributorEmailIds)
            {
                if (email.ToLower() == newUserEmailID.ToLower())
                {
                    isExistingUser = true;
                }
            }

            return isExistingUser;
        }

        [HttpPost]
        public async Task<IActionResult> SaveEmailAddress(AddUserEmailAddressModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("AddContributor", "UserManagement");
                return View("AddEmailAddress", model);
            }

            //check for rp user
            bool? isRpUser = await _userService.IsRpUserAsync(model.EmailAddress);

            if (isRpUser.Value == true)
            {
                ModelState.AddModelError(nameof(model.EmailAddress), "This user is already registered as a Responsible Party and cannot be assigned as a contributor or Designated Duty Holder under another organisation.");
                this.ShowBackButton("AddContributor", "UserManagement");
                return View("AddEmailAddress", model);
            }

            // if this email address exists in the existing users list then throw error
            bool isExistingUser = await DoesUserAlreadyExist(model.EmailAddress);
            if (isExistingUser)
            {
                ModelState.AddModelError(nameof(model.EmailAddress), "This user already has an active account. Go back and use Add an existing user to give them access.");
                this.ShowBackButton("AddContributor", "UserManagement");
                return View("AddEmailAddress", model);
            }

            // Logic to save email address goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
                m => m.AddUserEmailAddressModel = model,
                ContributorWorkflowStep.ContactDetails
            );

            return RedirectToAction("ContactDetails");
        }

        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.ContactDetails)]
        public IActionResult ContactDetails()
        {
            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();
            this.ShowBackButton("AddEmailAddress");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(state.Data.ContributorContactDetailsModel ?? new ContributorContactDetailsModel());
        }

        [HttpPost]
        public IActionResult SaveContactDetails(ContributorContactDetailsModel contactDetails)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ContactDetails");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                TempData["ErrorSummary"] = "Custom";
                return View("ContactDetails", contactDetails);
            }
            // Logic to save contact details goes here

            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
                m => m.ContributorContactDetailsModel = contactDetails,
                ContributorWorkflowStep.ChooseHeatNetwork
            );

            return RedirectToAction("ChooseHeatNetwork");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.ChooseHeatNetwork)]
        public async Task<IActionResult> ChooseHeatNetworkAsync()
        {
            //get heat networks from the database or service
            _logger.LogInformation("Retrieving heat networks for the user.");

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var response = await _userService.GetUserHeatNetworks(userId);
            var heatNetworks = await Utility.GetHeatNetworkSelectListAsync(response);

            if (heatNetworks == null)
            {
                _logger.LogError("No heat networks found in API for the UserId : {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to retrieve heat network information. Please try again later.";
                return View("Contributors/ChooseHeatNetwork");
            }


            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();

            var model = new ChooseHeatNetworkModel
            {
                HeatNetworks = heatNetworks,
                SelectedHeatNetworkId = state.Data?.ChooseHeatNetworkModel?.SelectedHeatNetworkId ?? null
            };

            this.ShowBackButton("ContactDetails");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            ViewBag.FormAction = "SaveChosenHeatNetwork";
            ViewBag.FormController = "NewContributor";
            return View("Contributor/ChooseHeatNetwork", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChosenHeatNetworkAsync(ChooseHeatNetworkModel model)
        {

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var response = await _userService.GetUserHeatNetworks(userId);
            var heatNetworks = await Utility.GetHeatNetworkSelectListAsync(response);

            if (heatNetworks == null)
            {
                _logger.LogError("No heat networks found in API for the UserId : {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to retrieve heat network information. Please try again later.";
                ViewBag.FormAction = "SaveChosenHeatNetwork";
                ViewBag.FormController = "NewContributor";
                return View("Contributor/ChooseHeatNetwork");
            }
            model.HeatNetworks = heatNetworks;

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ContactDetails");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("Contributor/ChooseHeatNetwork", model);
            }

            model.SelectedHeatNetworkName = model.HeatNetworks
                .FirstOrDefault(hn => hn.Value == model.SelectedHeatNetworkId)?.Text;

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
             m => m.ChooseHeatNetworkModel = model,
             ContributorWorkflowStep.ChooseRole
            );

            return RedirectToAction("ChooseRole");
        }


        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.ChooseRole)]
        public async Task<IActionResult> ChooseRole()
        {
            var model = new ChooseRoleModel();
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var user = await _userService.GetUserById(userId);
            var hnId = _workflowManager.GetState<AddNewContributorWorkflowModel>().Data.ChooseHeatNetworkModel.SelectedHeatNetworkId;

            var userRole = await Utility.GetUserRoleByUserHNMapping(user, hnId);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserRoleKey, userRole);
            var roles = Utility.GetContributorSelectList(userRole);
            if (roles == null)
            {
                _logger.LogError("No contributor roles found in API.");
                TempData["ErrorMessage"] = "Unable to retrieve contributor roles. Please try again later.";
                return View("Contributor/ChooseRole", model);
            }

            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();
            model.SelectedRoleId = state.Data?.ChooseRoleModel?.SelectedRoleId ?? null;
            model.Roles = roles;

            this.ShowBackButton("ChooseHeatNetwork");
            ViewBag.FormAction = "SaveChosenRole";
            ViewBag.FormController = "NewContributor";
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View("Contributor/ChooseRole", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChosenRoleAsync(ChooseRoleModel model)
        {
            ViewBag.FormAction = "SaveChosenRole";
            ViewBag.FormController = "NewContributor";
            var userRole = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserRoleKey);
            var roles = Utility.GetContributorSelectList(userRole);

            if (roles == null)
            {
                _logger.LogError("No contributor roles found in API.");
                TempData["ErrorMessage"] = "Unable to retrieve contributor roles. Please try again later.";
                return View("Contributors/ChooseRole", model);
            }
            model.Roles = roles;
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ChooseRole");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("Contributors/ChooseRole", model);
            }

            model.SelectedRoleName = model.Roles
             .FirstOrDefault(hn => hn.Value == model.SelectedRoleId)?.Text;

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
             m => m.ChooseRoleModel = model,
             ContributorWorkflowStep.Review
            );

            return RedirectToAction("CheckYourAnswers");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.Review)]
        public IActionResult CheckYourAnswers()
        {
            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();
            this.ShowBackButton("ChooseRole");

            var reviewModel = new ReviewSummaryModel
            {
                Sections = BuildReviewSections(state.Data)
            };
            ViewBag.FormAction = "SubmitAnswers";
            ViewBag.FormController = "NewContributor";
            return View("Contributor/CheckYourAnswers", reviewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswers()
        {
            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();
            if (state == null || state.Data == null)
            {
                _logger.LogError("Workflow state or data is null when trying to submit answers.");
                TempData["ErrorMessage"] = "Unable to submit your details. Please try again later.";
                return RedirectToAction("CheckYourAnswers");
            }

            TempData["FullName"] = $"{state.Data.ContributorContactDetailsModel.FirstName} {state.Data.ContributorContactDetailsModel.LastName}";
            TempData["HeatNetwork"] = state.Data.ChooseHeatNetworkModel.SelectedHeatNetworkName;
            TempData["CompanyName"] = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            _logger.LogInformation("Submitting new contributor details for user: {UserId}", state.Data.AddUserEmailAddressModel?.EmailAddress);

            var selectedContributorRole = (ContributorRole)Convert.ToInt32(state.Data.ChooseRoleModel.SelectedRoleId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            try
            {
                var invitationId = await _invitationService.AddInvitedUserAsync(
                       userId,
                       new AddInvitationRequest(
                           emailAddress: state.Data.AddUserEmailAddressModel.EmailAddress,
                           firstName: state.Data.ContributorContactDetailsModel.FirstName,
                           lastName: state.Data.ContributorContactDetailsModel.LastName,
                           hnId: state.Data.ChooseHeatNetworkModel.SelectedHeatNetworkId,
                           contributorRoles: new List<ContributorRole> { selectedContributorRole },
                           status: InvitationStatus.Invited
                       )
                   );

                if (string.IsNullOrWhiteSpace(invitationId))
                {
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                    return RedirectToAction("CheckYourAnswers");
                }

                _logger.LogInformation("Successfully submitted new contributor details.");
                var token = _iInvitationTokenService.GenerateToken(invitationId, state.Data.AddUserEmailAddressModel.EmailAddress);

                //send invitation email
                await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting new contributor details.");
                TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                return RedirectToAction("CheckYourAnswers");
            }

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.Confirmation);

            return RedirectToAction("Confirmation");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.Confirmation)]
        public IActionResult Confirmation()
        {

            // Retrieve the data from TempData.
            var fullName = TempData["FullName"] as string;
            var heatNetwork = TempData["HeatNetwork"] as string;
            var organisationName = TempData["OrganisationName"] as string;

            // You can use a ViewBag or ViewData to pass the data to the view.
            ViewData["FullName"] = fullName;
            ViewData["HeatNetwork"] = heatNetwork;
            ViewData["OrganisationName"] = organisationName;

            return View("Contributor/Confirmation");
        }




        private List<ReviewSection> BuildReviewSections(AddNewContributorWorkflowModel model)
        {
            var reviewSections = new List<ReviewSection>
            {
                new ReviewSection
                {
                    Heading = "Personal details",
                    Items = new List<ReviewItem>
                    {
                        new ReviewItem { Key = "First name", Value = model.ContributorContactDetailsModel?.FirstName, ChangeLink = Url.Action("ContactDetails"), ChangeLinkText = "Change" },
                        new ReviewItem { Key = "Last name", Value = model.ContributorContactDetailsModel?.LastName, ChangeLink = Url.Action("ContactDetails"), ChangeLinkText = "Change" }
                    }
                },
                new ReviewSection
                {
                    Heading = "Contact details",
                    Items = new List<ReviewItem>
                    {
                        new ReviewItem { Key = "Email address", Value = model.AddUserEmailAddressModel?.EmailAddress, ChangeLink = Url.Action("AddEmailAddress"), ChangeLinkText = "Change" }
                    }
                },
                new ReviewSection
                {
                    Heading = "Heat network information",
                    Items = new List<ReviewItem>
                    {
                        new ReviewItem { Key = "Heat network", Value = model.ChooseHeatNetworkModel?.SelectedHeatNetworkName, ChangeLink = Url.Action("ChooseHeatNetwork"), ChangeLinkText = "Change" },
                        new ReviewItem { Key = "Role assigned", Value = model.ChooseRoleModel?.SelectedRoleName, ChangeLink = Url.Action("ChooseRole"), ChangeLinkText = "Change" }
                    }
                }
            };

            return reviewSections;
        }
    }
}
