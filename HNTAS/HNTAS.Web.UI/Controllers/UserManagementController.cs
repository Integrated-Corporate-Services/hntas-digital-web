using HNTAS.Api.Client.Model;
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
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly ILogger<UserManagementController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IWorkflowManager _workflowManager;

        public UserManagementController(IUserService userService,
            ILogger<UserManagementController> logger,
            ISessionHelper sessionHelper,
            IWorkflowManager workflowManager,
            IHeatNetworkService heatNetworkService
            )
        {
            _logger = logger;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _workflowManager = workflowManager;
            _heatNetworkService = heatNetworkService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

                var users = await _userService.GetManagedUsers(userId);
                var contributorRoles = await _userService.GetContributorRolesAsync();
                var userRoles = await _userService.GetUserRolesAsync();
                var organisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                var allRoles = contributorRoles.Concat(userRoles).ToList();
                //var heatNetworks = await _heatNetworkService.GetAllHeatNetworks();
                var currentUserEmailId = "";

                var displayUsers = new List<UserDisplayModel>();

                foreach (var user in users)
                {
                    if(user.Id == userId)
                    {
                        currentUserEmailId = user.EmailId;
                    }
                    displayUsers.Add(new UserDisplayModel
                    {
                        Id = user.Id,
                        EmailAddress = user.EmailId,
                        Name = user.Name,
                        Roles = user.Roles.Select(r => allRoles.FirstOrDefault(cr => cr.Name == r.ToString()).Description).ToList(),
                        Status = user.Status.ToString(),
                        IsCurrentUser = user.Id == userId,
                        HeatNetworks = user.HeatNetworks.Select(hn => hn.Name).ToList()
                    });
                }

                // Required for Viewing User details
                var existingUserModel = _sessionHelper.GetFromSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey);
                if (existingUserModel == null)
                {
                    existingUserModel = new UserModel
                    {
                        ContactDetails =
                        {
                            EmailAddress = currentUserEmailId
                        }
                    };
                    _sessionHelper.SaveToSession(HttpContext, SessionKeys.UserCreation_SessionKey, existingUserModel);
                }
                // -----------

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
                return View("ManageUsers", new ManageUsersModel());
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

            this.ShowBackButton("UserAccount", "Dashboard");

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworksAsync()
        {

            this.ShowBackButton("UserAccount", "Dashboard");
            var user = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));

            ViewBag.UserRole = user?.Roles[0].ToString();
            ViewBag.HasDeclaredImpartiality = _sessionHelper.GetFromSession<DeclationOfImpartialityModel>(HttpContext, SessionKeys.DeclarationOfImpartialityModelKey)?.HasDeclaredImpartiality;

            if (user == null)
            {
                _logger.LogError("User not found in session or API.");
                TempData["ErrorMessage"] = "Unable to retrieve user information. Please try again later.";
                return View(new HeatNetworksViewModel());
            }

            var heatNetworks = new List<HeatNetworkModel>();

            if (user.HeatNetworks != null && user.HeatNetworks?.Count > 0)
            {
                foreach (var network in user.HeatNetworks)
                {
                    heatNetworks.Add(new HeatNetworkModel
                    {
                        HnId = network.HnId,
                        Name = network.Name,
                        OrganisationName = user.Organisation?.Name,
                        Status = "Active"
                    });
                }
            }

            var model = new HeatNetworksViewModel
            {
                HeatNetworks = heatNetworks,
                IsResponsiblePerson = user.Roles?.Contains(UserRole.ResponsiblePerson) ?? false,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkUserRolesAsync(string hnId)
        {
            if (string.IsNullOrEmpty(hnId))
            {
                _logger.LogError("Heat network ID is null or empty in HeatNetworkUserRoles.");
                TempData["ErrorMessage"] = "Invalid heat network ID.";
                return RedirectToAction("HeatNetworks");
            }

            var heatNetworkResponse = await _heatNetworkService.GetAsync(hnId.ToUpper());

            if (heatNetworkResponse == null)
            {
                // Log the ID that wasn't found before returning BadRequest
                _logger.LogWarning("Heat network not found for ID: {HeatNetworkId}", hnId);
                return BadRequest();
            }

            var userRolesDetailsResponse = await _userService.GetHeatNetworkUserRoles(hnId.ToUpper());


            var viewModel = new HeatNetworkUserRolesViewModel
            {
                HeatNetworkName = heatNetworkResponse.Name,
                UserRoles = userRolesDetailsResponse?.Select(x => new UserRoles
                {
                    RoleName = x.RoleDescription,
                    FullName = x.FullName,
                    EmailId = x.EmailId
                }).ToList() ?? []
            };

            this.ShowBackButton("HeatNetworks");
            return View("HeatNetworkUserRoles", viewModel);
        }
    }
}