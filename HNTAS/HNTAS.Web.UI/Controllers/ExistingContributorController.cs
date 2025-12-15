using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.Review;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class ExistingContributorController : Controller
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly ILogger<ExistingContributorController> _logger;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IInvitationTokenService _iInvitationTokenService;
        private readonly IInvitationService _invitationService;

        public ExistingContributorController(IWorkflowManager workflowManager,
            ILogger<ExistingContributorController> logger,
            IUserService userService,
            ISessionHelper sessionHelper,
            IInvitationTokenService invitationTokenService,
            IInvitationService invitationService)
        {
            _workflowManager = workflowManager;
            _logger = logger;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _iInvitationTokenService = invitationTokenService;
            _invitationService = invitationService;
        }

        [HttpGet]
        public async Task<IActionResult> ChooseUserAsync()
        {
            var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();

            state.Data.ChooseContributorModel ??= new ChooseContributorModel();
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            //Call API to get list of contributors
            var contributors = await GetContributorSelectListAsync(userId);
            if (contributors == null || !contributors.Any())
            {
                _logger.LogError("No contributors found for the current user.");
                ViewData["ErrorMessage"] = "No users found. Please contact support.";
                return View(state.Data.ChooseContributorModel);
            }

            state.Data.ChooseContributorModel.Contributors = contributors;
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            this.ShowBackButton("AddContributor", "UserManagement");
            return View(state.Data.ChooseContributorModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChooseUserAsync(ChooseContributorModel model)
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            model.Contributors = await GetContributorSelectListAsync(userId);

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("AddContributor", "UserManagement");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("ChooseUser", model);
            }

            model.SelectedContributorEmail = model.Contributors.FirstOrDefault(x => x.Value == model.SelectedContributorId).Text;
            // Logic to save contact details goes here
            _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(
                m => m.ChooseContributorModel = model,
                ExistingContributorWorkflowStep.ChooseHeatNetwork
            );

            var selectedUserData = await _userService.GetUserById(model.SelectedContributorId);

            var contactDetailsModel = new ContributorContactDetailsModel
            {
                FirstName = selectedUserData?.FirstName,
                LastName = selectedUserData?.LastName
            };

            _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(
               m => m.ContributorContactDetailsModel = contactDetailsModel,
               ExistingContributorWorkflowStep.ChooseHeatNetwork
            );

            return RedirectToAction("ChooseHeatNetwork");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(ExistingContributorWorkflowStep.ChooseHeatNetwork)]
        public async Task<IActionResult> ChooseHeatNetworkAsync()
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var response = await _userService.GetUserHeatNetworks(userId);
            var heatNetworks = await Utility.GetHeatNetworkSelectListAsync(response);

            if (heatNetworks == null)
            {
                _logger.LogError("No heat networks found in API for the UserId : {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to retrieve heat network information. Please try again later.";
                return View("Contributor/ChooseHeatNetwork");
            }

            var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();

            var model = new ChooseHeatNetworkModel
            {
                HeatNetworks = heatNetworks,
                SelectedHeatNetworkId = state.Data?.ChooseHeatNetworkModel?.SelectedHeatNetworkId ?? null
            };

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            ViewBag.FormAction = "SaveChooseHeatNetwork";
            ViewBag.FormController = "ExistingContributor";
            this.ShowBackButton("ChooseUser");
            return View("Contributor/ChooseHeatNetwork", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChooseHeatNetworkAsync(ChooseHeatNetworkModel model)
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var response = await _userService.GetUserHeatNetworks(userId);
            var heatNetworks = await Utility.GetHeatNetworkSelectListAsync(response);

            if (heatNetworks == null)
            {
                _logger.LogError("No heat networks found in API for the UserId : {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to retrieve heat network information. Please try again later.";
                ViewBag.FormAction = "SaveChooseHeatNetwork";
                ViewBag.FormController = "ExistingContributor";
                return View("Contributor/ChooseHeatNetwork");
            }
            model.HeatNetworks = heatNetworks;

            if (!ModelState.IsValid)
            {
                ViewBag.FormAction = "SaveChooseHeatNetwork";
                ViewBag.FormController = "ExistingContributor";
                this.ShowBackButton("ChooseUser");

                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("Contributor/ChooseHeatNetwork", model);
            }

            model.SelectedHeatNetworkName = model.HeatNetworks
                .FirstOrDefault(hn => hn.Value == model.SelectedHeatNetworkId)?.Text;

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(
             m => m.ChooseHeatNetworkModel = model,
             ExistingContributorWorkflowStep.ChooseRole
            );

            return RedirectToAction("ChooseRole");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(ExistingContributorWorkflowStep.ChooseRole)]
        public async Task<IActionResult> ChooseRole()
        {
            var model = new ChooseRoleModel();
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var user = await _userService.GetUserById(userId);
            var hnId = _workflowManager.GetState<AddExistingContributorWorkflowModel>().Data.ChooseHeatNetworkModel.SelectedHeatNetworkId;

            var userRole = await Utility.GetUserRoleByUserHNMapping(user, hnId);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserRoleKey, userRole);


            var roles = Utility.GetContributorSelectList(userRole);
            if (roles == null)
            {
                _logger.LogError("No contributor roles found in API.");
                TempData["ErrorMessage"] = "Unable to retrieve contributor roles. Please try again later.";
                return View("Contributor/ChooseRole", model);
            }
            var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();
            model.SelectedRoleId = state.Data?.ChooseRoleModel?.SelectedRoleId ?? null;
            model.Roles = roles;

            this.ShowBackButton("ChooseHeatNetwork");
            ViewBag.FormAction = "SaveChooseRole";
            ViewBag.FormController = "ExistingContributor";
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View("Contributor/ChooseRole", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChooseRole(ChooseRoleModel model)
        {
            ViewBag.FormAction = "SaveChooseRole";
            ViewBag.FormController = "ExistingContributor";

            var roles = await GetContributorRolesSelectListAsync();

            if (roles == null)
            {
                _logger.LogError("No contributor roles found in API.");
                TempData["ErrorMessage"] = "Unable to retrieve contributor roles. Please try again later.";
                return View("Contributor/ChooseRole", model);
            }
            model.Roles = roles;

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ChooseHeatNetwork");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("Contributor/ChooseRole", model);
            }

            model.SelectedRoleName = model.Roles
             .FirstOrDefault(hn => hn.Value == model.SelectedRoleId)?.Text;


            //check if the role is already assigned to another user for the selected heat network
            var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();
            (bool IsAssigned, string UserId) = await _userService.IsRoleAlreadyAssigned(state.Data.ChooseHeatNetworkModel.SelectedHeatNetworkId, model.SelectedRoleName);
            //check the role is present in the list or not
            if (IsAssigned)
            {
                _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(
                    m => m.ChooseRoleModel = model,
                    ExistingContributorWorkflowStep.ReplaceRoleConfirmation
                );
                return RedirectToAction("ReplaceUserRoleConfirmation");
            }
            else
            {
                // Logic to save details goes here
                _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(
                 m => m.ChooseRoleModel = model,
                 ExistingContributorWorkflowStep.CheckYourAnswers
                );
            }

            return RedirectToAction("CheckYourAnswers");
        }


        [HttpGet]
        [ValidateWorkflowStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(ExistingContributorWorkflowStep.CheckYourAnswers)]
        public IActionResult CheckYourAnswers()
        {
            var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();

            var reviewModel = new ReviewSummaryModel
            {
                Sections = BuildReviewSections(state.Data)
            };

            ViewBag.FormAction = "SubmitAnswers";
            ViewBag.FormController = "ExistingContributor";

            return View("Contributor/CheckYourAnswers", reviewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswersAsync()
        {
            var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();
            if (state == null || state.Data == null)
            {
                _logger.LogError("Workflow state or data is null when trying to submit answers.");
                TempData["ErrorMessage"] = "Unable to submit your details. Please try again later.";
                return RedirectToAction("CheckYourAnswers");
            }

            TempData["FullName"] = $"{state.Data.ContributorContactDetailsModel.FirstName} {state.Data.ContributorContactDetailsModel.LastName}";
            TempData["HeatNetwork"] = state.Data.ChooseHeatNetworkModel.SelectedHeatNetworkName;
            TempData["OrganisationName"] = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            _logger.LogInformation("Submitting new contributor details for user: {UserId}", state.Data.ChooseContributorModel?.SelectedContributorEmail);

            var selectedContributorRole = (ContributorRole)Convert.ToInt32(state.Data.ChooseRoleModel.SelectedRoleId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            try
            {
                var invitationId = await _invitationService.AddInvitedUserAsync(
                     userId,
                     new AddInvitationRequest(
                         emailAddress: state.Data.ChooseContributorModel?.SelectedContributorEmail,
                         firstName: state.Data.ContributorContactDetailsModel.FirstName,
                         lastName: state.Data.ContributorContactDetailsModel.LastName,
                         hnId: state.Data.ChooseHeatNetworkModel.SelectedHeatNetworkId,
                         contributorRoles: new List<ContributorRole> { selectedContributorRole },
                         replacedUserId: state.Data.ReplaceUserRoleViewModel != null ? state.Data.ReplaceUserRoleViewModel.CurrentRoleUserId : null,
                         rolesToReplace: new List<ContributorRole> { selectedContributorRole },
                         status: InvitationStatus.Invited
                     )
                 );

                if (string.IsNullOrWhiteSpace(invitationId))
                {
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                    return RedirectToAction("CheckYourAnswers");
                }

                _logger.LogInformation("Successfully submitted new contributor details for email: {Email}", state.Data.ChooseContributorModel.SelectedContributorEmail);
                var token = _iInvitationTokenService.GenerateToken(invitationId, state.Data.ChooseContributorModel?.SelectedContributorEmail);

                //send invitation email
                await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting new contributor details for email: {Email}", state.Data.ChooseContributorModel?.SelectedContributorEmail);
                TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                return RedirectToAction("CheckYourAnswers");
            }

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(ExistingContributorWorkflowStep.Confirmation);

            return RedirectToAction("Confirmation");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(ExistingContributorWorkflowStep.Confirmation)]
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


        [HttpGet]
        [ValidateWorkflowStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(ExistingContributorWorkflowStep.ReplaceRoleConfirmation)]
        public IActionResult ReplaceUserRoleConfirmation()
        {
            this.ShowBackButton("ChooseRole");
            var heatNetworkNameWithId = _workflowManager.GetState<AddExistingContributorWorkflowModel>().Data.ChooseHeatNetworkModel.SelectedHeatNetworkName;

            var model = new ReplaceUserRoleViewModel()
            {
                HeatNetworkName = heatNetworkNameWithId.Split("-")[1].Trim(),
                RoleName = _workflowManager.GetState<AddExistingContributorWorkflowModel>().Data.ChooseRoleModel.SelectedRoleName
            };
            ViewBag.ContollerName = "ExistingContributor";
            return View("Contributor/ReplaceUserRoleConfirmation", model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReplaceUserRoleAsync(ReplaceUserRoleViewModel replaceUserRoleViewModel)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ChooseRole");
                var heatNetworkNameWithId = _workflowManager.GetState<AddExistingContributorWorkflowModel>().Data.ChooseHeatNetworkModel.SelectedHeatNetworkName;

                replaceUserRoleViewModel.HeatNetworkName = heatNetworkNameWithId.Split("-")[1].Trim();
                replaceUserRoleViewModel.RoleName = _workflowManager.GetState<AddExistingContributorWorkflowModel>().Data.ChooseRoleModel.SelectedRoleName;

                return View("Contributor/ReplaceUserRoleConfirmation", replaceUserRoleViewModel);
            }

            if (replaceUserRoleViewModel.ReplaceExistingRole.ToUpper() == "YES")
            {
                var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();
                (bool IsAssigned, string UserId) = await _userService.IsRoleAlreadyAssigned(state.Data.ChooseHeatNetworkModel.SelectedHeatNetworkId, state.Data.ChooseRoleModel.SelectedRoleName);
                replaceUserRoleViewModel.CurrentRoleUserId = UserId;

                // Logic to save details goes here
                _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(
                 m => m.ReplaceUserRoleViewModel = replaceUserRoleViewModel,
                 ExistingContributorWorkflowStep.CheckYourAnswers
                );
                return RedirectToAction("CheckYourAnswers");
            }
            else
            {
                //set replaceUserRoleViewModel to null
                var state = _workflowManager.GetState<AddExistingContributorWorkflowModel>();

                // Logic to save details goes here
                _workflowManager.UpdateStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(
                 m => m.ReplaceUserRoleViewModel = null,
                 ExistingContributorWorkflowStep.CannotContinue
                );

                return RedirectToAction("CannotContinue");
            }
        }

        [HttpGet]
        [ValidateWorkflowStep<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>(ExistingContributorWorkflowStep.CannotContinue)]
        public IActionResult CannotContinue()
        {
            this.ShowBackButton("ChooseRole");
            ViewBag.HName = _workflowManager.GetState<AddNewContributorWorkflowModel>().Data.ChooseHeatNetworkModel.SelectedHeatNetworkName.Split("-")[1].Trim();
            return View("Contributor/CannotContinue");
        }


        private List<ReviewSection> BuildReviewSections(AddExistingContributorWorkflowModel model)
        {
            var reviewSections = new List<ReviewSection>
            {
                new ReviewSection
                {
                    Heading = "Personal details",
                    Items = new List<ReviewItem>
                    {
                        new ReviewItem { Key = "First name", Value = model.ContributorContactDetailsModel?.FirstName },
                        new ReviewItem { Key = "Last name", Value = model.ContributorContactDetailsModel?.LastName }
                    }
                },
                new ReviewSection
                {
                    Heading = "Contact details",
                    Items = new List<ReviewItem>
                    {
                        new ReviewItem { Key = "Email address", Value = model.ChooseContributorModel.SelectedContributorEmail, ChangeLink = Url.Action("ChooseUser"), ChangeLinkText = "Change" },
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

        private async Task<List<SelectItemOption>> GetContributorSelectListAsync(string userId)
        {
            // Call API to get list of contributors
            var contributors = await _userService.GetRegisteredUsersAsync(userId);

            // Check for null or empty list and return an empty list if necessary
            if (contributors == null || !contributors.Any())
            {
                _logger.LogWarning("No contributors found for the current user ID {UserId}.", userId);
                return new List<SelectItemOption>();
            }

            // Map contributors to a list of SelectListItem
            var selectedItems = contributors.Select(item => new SelectItemOption
            {
                Text = item.EmailId,
                Value = item.Id
            }).ToList();

            return selectedItems;
        }


        private async Task<List<SelectItemOption>?> GetContributorRolesSelectListAsync()
        {
            var response = await _userService.GetContributorRolesAsync();
            if (response == null) return null;
            return response.Select(hn => new SelectItemOption
            {
                Value = hn.Value.ToString(),
                Text = hn.Description
            }).ToList();
        }
    }
}
