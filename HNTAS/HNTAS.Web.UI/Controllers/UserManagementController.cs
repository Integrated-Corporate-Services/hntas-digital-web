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


        [HttpGet]
        public IActionResult AddContributor()
        {
            var viewModel = new AddContributorModel();
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
                    // Logic for choosing from an existing list
                    // For example: return RedirectToAction("ChooseFromExisting");
                }
            }

            ViewBag.OrganisationName = "Test Company Ltd";

            return View(viewModel);
        }
    }
}