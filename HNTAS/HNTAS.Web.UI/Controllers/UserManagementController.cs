using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserManagementController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IWorkflowManager _workflowManager;

        public UserManagementController(IUserService userService, ILogger<UserManagementController> logger, ISessionHelper sessionHelper, IWorkflowManager workflowManager)
        {
            _logger = logger;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _workflowManager = workflowManager;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

                var user = await _userService.GetManagedUsers(userId);
                var contributorRoles = await _userService.GetContributorRolesAsync();
                var userRoles = await _userService.GetUserRolesAsync();

                var organisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

                var displayUsers = new List<UserDisplayModel>();

                if (user.ResponsibleUser != null)
                {
                    displayUsers.Add(new UserDisplayModel
                    {
                        Id = user.ResponsibleUser.Id,
                        EmailAddress = user.ResponsibleUser.EmailId,
                        Name = user.ResponsibleUser.FullName,
                        Roles = user.ResponsibleUser.Roles.Select(r => userRoles.FirstOrDefault(cr => cr.Name == r.ToString()).Description).ToList(),
                        Status = user.ResponsibleUser.Status.ToString()
                    });
                }

                if (user.RegisteredUsers != null && user.RegisteredUsers.Count > 0)
                {
                    foreach (var contributor in user.RegisteredUsers)
                    {
                        displayUsers.Add(new UserDisplayModel
                        {
                            Id = contributor.Id,
                            EmailAddress = contributor.EmailId,
                            Name = contributor.FullName,
                            Roles = contributor.Roles.Select(r => userRoles.FirstOrDefault(cr => cr.Name == r.ToString()).Description).ToList(),
                            Status = contributor.Status.ToString()
                        });
                    }
                }

                if (user.InvitedUsers != null && user.InvitedUsers.Count > 0)
                {
                    foreach (var invited in user.InvitedUsers)
                    {
                        displayUsers.Add(new UserDisplayModel
                        {
                            Id = invited.Id,
                            EmailAddress = invited.Email,
                            Name = invited.FullName,
                            Roles = invited.Roles.Select(r => contributorRoles.FirstOrDefault(cr => cr.Name == r.ToString()).Description).ToList(),
                            Status = invited.Status.ToString()
                        });
                    }
                }

                var viewModel = new ManageUsersModel
                {
                    OrganisationName = organisationName,
                    Users = displayUsers
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


        [HttpGet]
        public IActionResult AddContributor()
        {
            var viewModel = new AddContributorModel();
            this.ShowBackButton("UserAccount", "Dashboard");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContributor(AddContributorModel viewModel)
        {
            if (viewModel.SelectedUserType == UserType.None)
            {
                ModelState.AddModelError("SelectedUserType", "Select how you want to add a contributor.");
            }

            if (ModelState.IsValid)
            {
                if (viewModel.SelectedUserType == UserType.NewUser)
                {
                    // Initiate the workflow for adding a new contributor
                    _workflowManager.StartWorkflow<AddNewContributorWorkflowModel>(WorkflowType.AddNewContributor, ContributorWorkflowStep.AddEmailAddress);
                    _logger.LogInformation("Created new workflow state for {WorkflowType}", WorkflowType.AddNewContributor);

                    return RedirectToAction("AddEmailAddress", "NewContributor");
                }
                else if (viewModel.SelectedUserType == UserType.ExistingUser)
                {
                    _workflowManager.StartWorkflow<AddExistingContributorWorkflowModel>(WorkflowType.AddExistingContributor, ExistingContributorWorkflowStep.ChooseUser);
                    _logger.LogInformation("Created new workflow state for {WorkflowType}", WorkflowType.AddExistingContributor);
                    return RedirectToAction("ChooseUser", "ExistingContributor");
                }
            }

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(viewModel);
        }
    }
}