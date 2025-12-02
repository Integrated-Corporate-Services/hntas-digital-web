using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.OrganisationRole;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace HNTAS.Web.UI.Controllers
{
    public class ExistingOrganisationUserController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IWorkflowManager _workflowManager;
        private readonly IUserService _userService;
        private readonly ILogger<ExistingOrganisationUserController> _logger;
        private readonly IInvitationService _invitationService;
        private readonly IOrganisationUserService _organisationUserService;

        public ExistingOrganisationUserController(
            ISessionHelper sessionHelper,
            IWorkflowManager workflowManager,
            IUserService userService,
            ILogger<ExistingOrganisationUserController> logger,
            IInvitationService invitationService,
            IOrganisationUserService organisationUserService)
        {
            _sessionHelper = sessionHelper;
            _workflowManager = workflowManager;
            _userService = userService;
            _logger = logger;
            _invitationService = invitationService;
            _organisationUserService = organisationUserService;
        }

        [HttpGet]
        public async Task<IActionResult> ChooseUser()
        {
            this.ShowBackButton("ChangeOrganisationUser", "UserManagment");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            // In a real application, you would retrieve the list of users from a database or service.

            var users = await _userService.GetUsersByOrganisationIdAsync(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId) ?? string.Empty);
            var userRoles = await _userService.GetUserRolesAsync();
            if (users == null || !users.Any())
            {
                _logger.LogWarning("No users found for organisation ID: {OrganisationId}", _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId));
                // Handle the case where no users are found, e.g., redirect to an error page or show a message.
                throw new Exception("No users found for the organisation.");
            }

            var userOptions = new List<SelectItemOption>();

            foreach (var user in users.Where(u => u.Roles.Contains(Api.Client.Model.UserRole.ResponsiblePerson) || u.Roles.Contains(Api.Client.Model.UserRole.Coordinator)))
            {
                var roles = user.Roles?.Select(r => userRoles.FirstOrDefault(cr => cr.Name == r.ToString())?.Description).ToList();
                userOptions.Add(new SelectItemOption
                {
                    Value = user.Id,
                    Text = $"{user.FullName} - ({string.Join(", ", roles)})"
                });
            }

            var model = new ChooseUserModel()
            {
                Users = userOptions
            };



            return View("ChooseUser", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveChooseUser(ChooseUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ChooseUser", model);
            }

            _workflowManager.UpdateStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(
             m => m.ChooseUserModel = model,
             ExistingOrganisationUserWorkflowStep.AssignRole
            );

            return RedirectToAction("AssignRole");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(ExistingOrganisationUserWorkflowStep.AssignRole)]
        public async Task<IActionResult> AssignRole()
        {
            this.ShowBackButton("ContactDetails");
            var state = _workflowManager.GetState<AddExistingOrganisationUserWorkflowModel>();
            //Get RP UserName for display
            var rpUser = await _organisationUserService.GetResponsiblePartyDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId));
            if (rpUser == null)
            {
                return BadRequest("Responsible Party details not found.");
            }

            //get selected user details
            var selectedUserId = state.Data.ChooseUserModel?.SelectedUserId;
            var selectedUser = await _userService.GetUserById(selectedUserId ?? string.Empty);
            if (selectedUser == null)
            {
                return BadRequest("Selected user details not found.");
            }
            var model = state.Data.RoleAssignmentModel ?? new RoleAssignmentModel()
            {
                ExistingRPName = rpUser?.FullName,
                UserName = $"{selectedUser?.FirstName} {selectedUser?.LastName}",
            };
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View("Contributor/AssignRole", state.Data.RoleAssignmentModel ?? model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAssignRole(RoleAssignmentModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ContactDetails");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("Contributor/AssignRole", model);
            }

            _workflowManager.UpdateStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(
             m => m.RoleAssignmentModel = model,
             ExistingOrganisationUserWorkflowStep.ReplaceRoleConfirmation
            );

            return RedirectToAction("ReplaceUserRoleConfirmation");
        }


        [HttpGet]
        [ValidateWorkflowStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(ExistingOrganisationUserWorkflowStep.ReplaceRoleConfirmation)]
        public IActionResult ReplaceUserRoleConfirmation()
        {
            this.ShowBackButton("ChooseUser");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View("ReplaceUserRoleConfirmation", new ReplaceUserRoleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReplaceUserRoleConfirmation(ReplaceUserRoleViewModel model)
        {

            var state = _workflowManager.GetState<AddExistingOrganisationUserWorkflowModel>();

            if (state == null || state.Data == null)
            {
                _logger.LogError("Workflow state or data is null when trying to submit answers.");
                TempData["ErrorMessage"] = "Unable to submit your details. Please try again later.";
                return RedirectToAction("ReplaceUserRoleConfirmation");
            }

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ChooseUser");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("ReplaceUserRoleConfirmation", model);
            }

            if (model.ReplaceExistingRole.ToUpper() == "YES")
            {

                try
                {
                    //Get selected user details

                    var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey) ?? string.Empty;
                    var selectedUserId = state.Data.ChooseUserModel?.SelectedUserId;
                    var selectedUser = await _userService.GetUserById(selectedUserId ?? string.Empty);
                    if (selectedUser == null)
                    {
                        return BadRequest("Selected user details not found.");
                    }
                    var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId) ?? string.Empty;

                    var selectedContributorRole = state.Data.RoleAssignmentModel?.SelectedRoleType == Models.Enums.RoleAssignmentType.HNTASCoordinator
                        ? ContributorRole.Coordinator
                        : ContributorRole.ResponsiblePerson;

                    TempData["UserName"] = $"{selectedUser?.FirstName} {selectedUser?.LastName}";
                    TempData["OrganisationName"] = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                    TempData["AssignedRole"] = state.Data.RoleAssignmentModel?.SelectedRoleType == Models.Enums.RoleAssignmentType.HNTASCoordinator ? "HNTAS Coordinator" : "Responsible Party";

                    var invitationId = await _invitationService.AddInvitedUserAsync(
                           userId,
                           new AddInvitationRequest(
                               emailAddress: selectedUser?.EmailId,
                               firstName: selectedUser?.FirstName,
                               lastName: selectedUser?.LastName,
                               contributorRoles: new List<ContributorRole> { selectedContributorRole },
                               orgId: orgId,
                               currentRoleUserId: null,
                               status: InvitationStatus.Invited
                           )
                       );

                    if (string.IsNullOrWhiteSpace(invitationId))
                    {
                        TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                        return RedirectToAction("ReplaceUserRoleConfirmation");
                    }

                    _logger.LogInformation("Successfully submitted new organisation user details.");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting new contributor details.");
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                    return RedirectToAction("ReplaceUserRoleConfirmation");
                }

                _workflowManager.UpdateStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(
                    ExistingOrganisationUserWorkflowStep.ReplaceRoleConfirmation
                );

                //show confirmation page
                return RedirectToAction("RoleAssignmentConfirmation");

            }
            else
            {

                _workflowManager.UpdateStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(
                ExistingOrganisationUserWorkflowStep.CannotContinue
                );
                return RedirectToAction("CannotContinue");
            }
            // Logic to replace user role would go here
        }

        [HttpGet]
        [ValidateWorkflowStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(ExistingOrganisationUserWorkflowStep.CannotContinue)]
        public IActionResult CannotContinue()
        {
            this.ShowBackButton("ReplaceUserRoleConfirmation");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View("CannotContinue");
        }


        [HttpGet]
        [ValidateWorkflowStep<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>(ExistingOrganisationUserWorkflowStep.ReplaceRoleConfirmation)]
        public IActionResult RoleAssignmentConfirmation()
        {
            // Retrieve the data from TempData.
            var fullName = TempData["UserName"] as string;
            var assignedRole = TempData["AssignedRole"] as string;
            var organisationName = TempData["OrganisationName"] as string;

            // You can use a ViewBag or ViewData to pass the data to the view.
            ViewData["FullName"] = fullName;
            ViewData["AssignedRole"] = assignedRole;
            ViewData["OrganisationName"] = organisationName;

            return View("Contributor/RoleAssignmentConfirmation");
        }
    }
}
