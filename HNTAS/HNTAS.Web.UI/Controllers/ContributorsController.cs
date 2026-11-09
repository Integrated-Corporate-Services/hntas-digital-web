using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Components;
using HNTAS.Web.UI.Models.Contributors;
using HNTAS.Web.UI.Models.NetworkLeads;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using CheckYourAnswersViewModel = HNTAS.Web.UI.Models.Contributors.CheckYourAnswersViewModel;

namespace HNTAS.Web.UI.Controllers
{
    public class ContributorsController : Controller
    {

        private readonly ISessionHelper _sessionHelper;
        private readonly ILogger<ContributorsController> _logger;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly IInvitationTokenService _invitationTokenService;
        private readonly IHeatNetworkService _heatNetworkService;

        public ContributorsController(ISessionHelper sessionHelper, ILogger<ContributorsController> logger, IUserService userService, IInvitationService invitationService, IInvitationTokenService invitationTokenService, IHeatNetworkService heatNetworkService)
        {
            _sessionHelper = sessionHelper;
            _logger = logger;
            _userService = userService;
            _invitationService = invitationService;
            _invitationTokenService = invitationTokenService;
            _heatNetworkService = heatNetworkService;
        }

        public async Task<IActionResult> ManageContributors([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "name", [FromQuery] string sortDirection = "asc")
        {
            // Fetch active organisation ID from session
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);
            var orgName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            var contributorsList = new List<ContributorItemDto>();
            int totalCount = 0;

            ViewBag.WhoDoYouWantToAdd = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey) ?? "Duty holders and contributors";

            if (!string.IsNullOrEmpty(orgId))
            {
                // Call your service method to retrieve paginated contributors
                var pagedResult = await _invitationService.GetOrganisationGroupedDdhAndContributor(
                    orgId,
                    pageNumber,
                    pageSize,
                    sortBy,
                    sortDirection);

                if (pagedResult != null)
                {
                    totalCount = pagedResult.TotalCount ?? 0;

                    if (pagedResult.Items != null && pagedResult.Items.Any())
                    {
                        // Map ContributorResult domain/API items to ContributorItemDto for the view
                        contributorsList = pagedResult.Items.Select(c => new ContributorItemDto
                        {
                            Id = c.UserId,
                            Name = c.Name,
                            Email = c.Email,
                            OrganisationName = c.WorkingOrgName ?? orgName
                        }).ToList();
                    }
                }
            }

            var viewModel = new ManageContributorsViewModel
            {
                Contributors = contributorsList,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            ViewBag.OrganisationName = orgName;

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> ViewContributor([FromQuery] string id, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "firstName",
            [FromQuery] string sortDirection = "asc")
        {
            try
            {
                // Validate and sanitize inputs
                if (pageNumber < 1) pageNumber = 1;

                // Validate sort order
                sortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";

                var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

                var managedUsers = await _userService.GetDdhAndContributorsPaginated(id, orgId, pageNumber: pageNumber, pageSize: pageSize, sortBy: sortBy, sortDirection: sortDirection);
                List<DDHAndContributorsListModel> listOfContributors = new List<DDHAndContributorsListModel>();
                var userRoles = await _userService.GetUserRolesAsync();

                foreach (var user in managedUsers.Items!)
                {
                    var primaryRoleName = user.Roles?.FirstOrDefault();

                    foreach (var heatNetwork in user.HeatNetworks)
                    {
                        listOfContributors.Add(new DDHAndContributorsListModel
                        {
                            Name = user.Name,
                            HeatNetworkId = heatNetwork.HnId,
                            HeatNetworkName = heatNetwork.Name,
                            Role = userRoles.FirstOrDefault(ur => ur.Name == primaryRoleName)?.Description,
                            Status = new InvitationStatusTag(user.Status)
                        });
                    }
                }
                
                ViewBag.WhoDoYouWantToAdd = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey) ?? "Duty holders and contributors";

                // Pass sorting and pagination info to view
                ViewBag.CurrentSort = sortBy;
                ViewBag.CurrentOrder = sortDirection;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = managedUsers.TotalPages ?? 1;
                ViewBag.TotalItems = managedUsers.TotalCount ?? 0;
                ViewBag.UserId = id;
                return View(listOfContributors);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DDH and contributors");
                TempData["ErrorMessage"] = "An error occurred while retrieving the DDH and contributors";

                // Return empty result
                var emptyResult = new List<DDHAndContributorsListModel>();

                ViewBag.CurrentSort = sortBy;
                ViewBag.CurrentOrder = sortDirection ?? "asc";
                ViewBag.CurrentPage = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 0;
                ViewBag.TotalItems = 0;
                ViewBag.NextOrder = "desc";

                return View(emptyResult);
            }
            
        }

        public ActionResult Start()
        {

            //clear session data for new lead invitation
            //_sessionHelper.ClearFromSession(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SameOrganisationViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.CheckYourAnswersContributorsModelSessionKey);

            var whoDoYouWantToAdd = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey) ?? "Duty holders and contributors";

            if (whoDoYouWantToAdd == "Duty holders and contributors")
            {
                return RedirectToAction(nameof(NewContributorRole));
            }
            return RedirectToAction(nameof(AddContributor));
        }

        [HttpGet]
        public IActionResult NewContributorRole()
        {
            this.ShowBackButton("ManageContributors");
            var model = _sessionHelper.GetFromSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey) ?? new NewContributorRoleViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewContributorRole(NewContributorRoleViewModel model)
        {
            this.ShowBackButton("ManageContributors");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey, model);            
            return RedirectToAction("AddContributor");
        }

        private string GetRole()
        {
            string? whoDoYouWantToAdd =
                _sessionHelper.GetFromSession<string>(
                    HttpContext,
                    SessionKeys.WhoDoYouWantToAddSessionKey);

            if (whoDoYouWantToAdd == null)
            {
                var roleModel =
                    _sessionHelper.GetFromSession<NewContributorRoleViewModel>(
                        HttpContext,
                        SessionKeys.NewContributorRoleViewModelSessionKey);

                if (roleModel == null)
                {
                    throw new InvalidOperationException(
                        $"Session key '{SessionKeys.NewContributorRoleViewModelSessionKey}' not found.");
                }

                whoDoYouWantToAdd = roleModel.IsDDH.Value
                    ? ContributorRole.DesignatedDutyHolder.GetDisplayName()
                    : ContributorRole.Contributor.GetDisplayName();
            }
            else
            {
                whoDoYouWantToAdd = ContributorRole.Contributor.GetDisplayName();
            }

            return whoDoYouWantToAdd;
        }

        [HttpGet]
        public IActionResult AddContributor()
        {            
            ViewBag.whoDoYouWantToAdd = GetRole();
            var backAction = GetRole() == ContributorRole.DesignatedDutyHolder.GetDisplayName() ? "NewContributorRole" : "ManageContributors";
            this.ShowBackButton(backAction);
            var model = _sessionHelper.GetFromSession<AddContributorViewModel>(HttpContext, SessionKeys.AddContributorViewModelSessionKey) ?? new AddContributorViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContributor(AddContributorViewModel model)
        {
            this.ShowBackButton("NewContributorRole");
            ViewBag.whoDoYouWantToAdd = GetRole();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string nextAction  = model.InviteNewContributor switch
            {
                true => "NewContributorEmail",
                false => "ExistingContributorsList"
            };
            _sessionHelper.SaveToSession<AddContributorViewModel>(HttpContext, SessionKeys.AddContributorViewModelSessionKey, model);
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult NewContributorEmail()
        {
            var viewModel = _sessionHelper.GetFromSession<NewContributorEmail>(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey) ?? new NewContributorEmail();
            this.ShowBackButton(nameof(AddContributor));
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNewContributorEmail(NewContributorEmail model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton(nameof(AddContributor));
                return View(nameof(NewContributorEmail), model);
            }

            //Check if the email address is already active in the system
            var existingUser = await _userService.GetUserByEmailIdAsync(model.EmailAddress);
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            //check if the user is rp
            if (existingUser != null && existingUser.Roles.Contains(UserRole.ResponsibleParty))
            {
                ModelState.AddModelError("EmailAddress", "This user cannot be added as a " + GetRole());
                return View(nameof(NewContributorEmail), model);
            }
            else if (existingUser != null
            && existingUser.ContributingOrganisations?.Contains(orgId!) == true
            && (existingUser.Roles.Contains(UserRole.NetworkManager)))
            {
                ModelState.AddModelError("EmailAddress", "This user cannot be added as a " + GetRole());

                return View(nameof(NewContributorEmail), model);
            }
            
            else if (existingUser != null)
            {
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey, model);
                //redirect to profile found page
                return RedirectToAction("NewContributorAlreadyExists");
            }

            // Save the invited email to session or proceed to next flow
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey, model);

            return RedirectToAction(nameof(NewContributorDetails));
        }


        [HttpGet]
        public async Task<IActionResult> NewContributorAlreadyExists()
        {
            // Retrieve existing profile match details from session/database lookup
            var emailModel = _sessionHelper.GetFromSession<NewContributorEmail>(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey);
            var organisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            if (emailModel == null)
            {
                TempData["ErrorMessage"] = "There was an error retrieving the existing user details. Please try again later.";
                return RedirectToAction(nameof(ManageContributors));
            }
            ;

            var existingUser = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);

            var viewModel = new NewContributorAlreadyExistsViewModel
            {
                FirstName = existingUser?.FirstName,
                LastName = existingUser?.LastName,
                EmailAddress = existingUser?.EmailId,
                JobTitle = existingUser?.JobTitle
            };

            this.ShowBackButton(nameof(NewContributorEmail));

            return View(nameof(NewContributorAlreadyExists), viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNewContributorAlreadyExists(NewContributorAlreadyExistsViewModel model)
        {
            // Repopulate metadata for redisplay if validation fails
            var emailModel = _sessionHelper.GetFromSession<NewContributorEmail>(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey);

            if (!ModelState.IsValid)
            {
                var existingUser = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);

                model.FirstName = existingUser?.FirstName;
                model.LastName = existingUser?.LastName;
                model.EmailAddress = existingUser?.EmailId;
                model.JobTitle = existingUser?.JobTitle;

                this.ShowBackButton(nameof(NewContributorEmail));
                return View(nameof(NewContributorAlreadyExists), model);
            }

            if (model.IsCorrectPerson == true)
            {
                //fill new lead details view model with existing user details and save to session
                // check for same organisation and redirect accordingly
                var existingUser = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);

                var newLeadDetailsModel = new NewContributorDetailsViewModel
                {
                    FirstName = existingUser?.FirstName,
                    LastName = existingUser?.LastName,
                    JobTitle = existingUser?.JobTitle,
                    WorkingOrgName = existingUser?.WorkingOrgName,
                    InviteForFirstTime = false
                };

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey, newLeadDetailsModel);

                return RedirectToAction(nameof(NewContributorHeatNetwork));
            }
            else
            {
                // Redirect back to enter a different email address
                return RedirectToAction(nameof(NewContributorEmail));
            }
        }

        [HttpGet]
        public IActionResult NewContributorDetails()
        {
            this.ShowBackButton(nameof(NewContributorEmail));
            ViewBag.whoDoYouWantToAdd = GetRole();
            var model = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey) ?? new NewContributorDetailsViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewContributorDetails(NewContributorDetailsViewModel model)
        {
            this.ShowBackButton(nameof(NewContributorEmail));
            ViewBag.whoDoYouWantToAdd = GetRole();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.InviteForFirstTime = true;

            var emailModel = _sessionHelper.GetFromSession<NewContributorEmail>(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey);

            var invitedUserDetails = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);
            var invitedRoleData = _sessionHelper.GetFromSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey);
            var currentUserOrgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            _sessionHelper.SaveToSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey, model);
            _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "NewContributorDetails");
            return RedirectToAction("NewContributorHeatNetwork");
        }

        [HttpGet]
        public IActionResult SameOrganisation()
        {
            var viewModel = _sessionHelper.GetFromSession<SameOrganisationViewModel>(
                HttpContext,
                SessionKeys.SameOrganisationViewModelSessionKey) ?? new SameOrganisationViewModel();
            var newContributorDetails = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(
                HttpContext,
                SessionKeys.NewContributorDetailsViewModelSessionKey);

            var backAction = newContributorDetails.InviteForFirstTime ? nameof(HeatNetworkPhase) : nameof(NewContributorAlreadyExists);

            this.ShowBackButton(backAction);

            ViewBag.Role = GetRole().ToLower();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSameOrganisationAsync(SameOrganisationViewModel model)
        {
            var role = GetRole().ToLower();

            if(model.WorksForSameOrganisation == null)
            {
                ModelState.AddModelError(
                   nameof(model.WorksForSameOrganisation),
                   $"Select yes whether you and the {role} work for the same organisation");
            }

            if (model.WorksForSameOrganisation == false &&
                string.IsNullOrWhiteSpace(model.OrganisationName))
            {
                ModelState.AddModelError(
                    nameof(model.OrganisationName),
                    "Enter the organisation name");
            }

            if (!ModelState.IsValid)
            {
                var newContributorDetails = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(
                 HttpContext,
                 SessionKeys.NewContributorDetailsViewModelSessionKey);

                ViewBag.Role = role;

                var backAction = newContributorDetails.InviteForFirstTime ? nameof(NewContributorDetails) : nameof(NewContributorAlreadyExists);

                this.ShowBackButton(backAction);
                return View(nameof(SameOrganisation), model);
            }

            if (model.WorksForSameOrganisation == true)
            {
                model.OrganisationName = null;
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SameOrganisationViewModelSessionKey, model);

            return RedirectToAction(nameof(CheckYourAnswers));
        }

        private async Task<List<NewContributorDetailsViewModel>> GetContributorSelectListAsync()
        {
            var userIdFromSession = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var orgIdFromSession = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            // Call API to get list of contributors
            var contributors = await _userService.GetRegisteredUsersAsync(userIdFromSession);

            
            // Check for null or empty list and return an empty list if necessary
            if (contributors == null || !contributors.Any())
            {
                _logger.LogWarning("No contributors found for the current user ID {UserId}.", userIdFromSession);
                return null;
            }

            // Map contributors to a list of SelectListItem
            var selectedUsers = contributors.Select(user => new NewContributorDetailsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailAddress = user.EmailId,
                JobTitle = user.JobTitle,
                WorkingOrgName = user.WorkingOrgName,
                InviteForFirstTime = false
            }).ToList();
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey, new ExistingContributorsListViewModel { Contributors = selectedUsers });
            return selectedUsers;
        }

        [HttpGet]
        public async Task<IActionResult> ExistingContributorsList()
        {
            this.ShowBackButton("AddContributor");
            var model = _sessionHelper.GetFromSession<ExistingContributorsListViewModel>(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey) ?? new ExistingContributorsListViewModel { Contributors = await GetContributorSelectListAsync() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExistingContributorsList(ExistingContributorsListViewModel model)
        {
            this.ShowBackButton("AddContributor");
            model.Contributors = await GetContributorSelectListAsync();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            NewContributorDetailsViewModel selectedUser = model.Contributors.FirstOrDefault(x => x.EmailAddress == model.SelectedEmailAddress);
            model.SelectedUser = selectedUser;
            _sessionHelper.SaveToSession<ExistingContributorsListViewModel>(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey, model);
            _sessionHelper.SaveToSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey, selectedUser);
            _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "ExistingContributorsList");
            return RedirectToAction("NewContributorHeatNetwork");
        }

        private async Task<List<SelectItemOption>> GetListOfHeatNetworks()
        {
            //get heat networks from the database or service
            _logger.LogInformation("Retrieving heat networks for the user.");

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);
            var response = await _userService.GetUserHeatNetworks(userId);

            //filter ddh role only so, it won't list the user contibution networks
            var respectiveNetworks = response?.Where(w => w.OrgId == orgId && w.Role != UserRole.Contributor.ToString()).ToList();
            var heatNetworks = await Utility.GetHeatNetworkSelectListAsync(respectiveNetworks!);

            if (heatNetworks == null)
            {
                _logger.LogError("No heat networks found in API for the UserId : {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to retrieve heat network information. Please try again later";
                return null;
            }
            return heatNetworks;
        }

        [HttpGet]
        public async Task<IActionResult> NewContributorHeatNetwork()
        {
            var addContributorDetailModel = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey);
            var existingContributor = _sessionHelper.GetFromSession<ExistingContributorsListViewModel>(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey);

            if(existingContributor == null)
            {
                this.ShowBackButton(addContributorDetailModel?.InviteForFirstTime == true ? nameof(NewContributorDetails) : nameof(NewContributorAlreadyExists));
            }
            else
            {
                this.ShowBackButton(nameof(ExistingContributorsList));
            }
   
            var model = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey) ?? new NewContributorHeatNetworkViewModel();
            model.HeatNetworks = await GetListOfHeatNetworks();
            if (model.HeatNetworks == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                _sessionHelper.SaveToSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey, model);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewContributorHeatNetwork(NewContributorHeatNetworkViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var addContributorDetailModel = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey);
                var existingContributor = _sessionHelper.GetFromSession<ExistingContributorsListViewModel>(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey);

                if (existingContributor == null)
                {
                    this.ShowBackButton(addContributorDetailModel!.InviteForFirstTime == true ? nameof(NewContributorDetails) : nameof(NewContributorAlreadyExists));
                }
                else
                {
                    this.ShowBackButton(nameof(ExistingContributorsList));
                }

                model = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
                return View(model);
            }
            _sessionHelper.SaveToSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey, model);
            var network = await _heatNetworkService.GetAsync(model.SelectedHeatNetwork);
            if(network.RegistrationSource == RegistrationSource.OFGEM)
            {
                HeatNetworkPhaseViewModel phaseModel = new HeatNetworkPhaseViewModel { Phases = [], SelectedPhases = ["Operation"] };
                _sessionHelper.SaveToSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey, phaseModel);
                _sessionHelper.SaveToSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey, RegistrationSource.OFGEM);
                _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "NewContributorHeatNetwork");
                return RedirectToAction("CheckYourAnswers");
            }
            return RedirectToAction("HeatNetworkPhase");
        }

        private List<SelectItemOption> GetListOfHeatNetworkPhases()
        {
            return new List<SelectItemOption>
            {
                new SelectItemOption
                {
                    Value = "feasibility",
                    Text = "Feasibility",
                    Hint = "Concept design"
                },
                new SelectItemOption
                {
                    Value = "design",
                    Text = "Design",
                    Hint = "Developed design, technical design"
                },
                new SelectItemOption
                {
                    Value = "construction",
                    Text = "Construction",
                    Hint = "Construction design, installation, commissioning"
                },
                new SelectItemOption
                {
                    Value = "operation",
                    Text = "Operation",
                    Hint = "Operation, maintenance, ongoing monitoring"
                }
            };
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkPhase()
        {
            this.ShowBackButton("NewContributorHeatNetwork");
            var hnModel = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
            var network = await _heatNetworkService.GetAsync(hnModel.SelectedHeatNetwork);            
            HeatNetworkPhaseViewModel model;
            if (network.RegistrationSource != RegistrationSource.OFGEM) {
                model = _sessionHelper.GetFromSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey) ?? new HeatNetworkPhaseViewModel { Phases = GetListOfHeatNetworkPhases() };
                _sessionHelper.SaveToSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey, RegistrationSource.HNTAS);
            }
            else {
                model = _sessionHelper.GetFromSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey) ?? new HeatNetworkPhaseViewModel { Phases = GetListOfHeatNetworkPhases() };
            }                
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkPhase(HeatNetworkPhaseViewModel model)
        {
            this.ShowBackButton("NewContributorHeatNetwork");
            model.Phases = GetListOfHeatNetworkPhases();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey, model);
            _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "HeatNetworkPhase");

            var userdetailsModel = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey);

            if (userdetailsModel?.InviteForFirstTime == true)
            {
                return RedirectToAction("SameOrganisation");
            };

            return RedirectToAction("CheckYourAnswers");
        }

        private CheckYourAnswersViewModel CreateCYAModel()
        {
            var newContributorEmailModel = _sessionHelper.GetFromSession<NewContributorEmail>(HttpContext, SessionKeys.NewContributorEmailViewModelSessionKey);
            var newContributorRoleModel = _sessionHelper.GetFromSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey);
            
            var newContributorDetailsModel = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey);
            var newContributorHeatNetworkModel = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey);

            var existingContributor = _sessionHelper.GetFromSession<ExistingContributorsListViewModel>(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey);

            var sameOrgDetails = _sessionHelper.GetFromSession<SameOrganisationViewModel>(
              HttpContext,
              SessionKeys.SameOrganisationViewModelSessionKey);

            var newModel = new CheckYourAnswersViewModel
            {
                FirstName = newContributorDetailsModel?.FirstName,
                LastName = newContributorDetailsModel?.LastName,
                JobTitle = newContributorDetailsModel?.JobTitle,
                EmailAddress = existingContributor == null ? newContributorEmailModel?.EmailAddress : existingContributor.SelectedEmailAddress,
                InviteForFirstTime = newContributorDetailsModel?.InviteForFirstTime ?? false,
                RoleAssigned = newContributorRoleModel?.IsDDH == true ? "designated-duty holder" : "contributor",
                HeatNetwork = newContributorHeatNetworkModel.SelectedHeatNetwork,
                SelectedPhases = heatNetworkPhaseModel?.SelectedPhases,
                WorkForSameOrganisation = sameOrgDetails?.WorksForSameOrganisation,
                WorkingOrganisationName = sameOrgDetails?.OrganisationName,
            };
            return newModel;
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backAction");
            this.ShowBackButton(backAction);
            ViewBag.RegistrationSource = _sessionHelper.GetFromSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey);

            var addContributorModel = _sessionHelper.GetFromSession<AddContributorViewModel>(HttpContext, SessionKeys.AddContributorViewModelSessionKey);
            ViewBag.ChangeEmailAction = addContributorModel!.InviteNewContributor == true ? nameof(NewContributorEmail) : "ExistingContributorsList";
            var model = _sessionHelper.GetFromSession<CheckYourAnswersViewModel>(HttpContext, SessionKeys.CheckYourAnswersContributorsModelSessionKey) ?? CreateCYAModel();            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckYourAnswers(CheckYourAnswersViewModel model)
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backAction");
            this.ShowBackButton(backAction);
            ViewBag.RegistrationSource = _sessionHelper.GetFromSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey);
            model = CreateCYAModel();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var inviteeRole = model.RoleAssigned == "contributor" ? ContributorRole.Contributor : ContributorRole.DesignatedDutyHolder;            
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            var newContributorDetailsModel = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey);

            try
            {
                var invitationId = await _invitationService.AddInvitedUserAsync(
                     userId,
                     new AddInvitationRequest(
                         emailAddress: model.EmailAddress,
                         firstName: model.FirstName,
                         lastName: model.LastName,
                         hnId: model.HeatNetwork,
                         contributorRoles: new List<ContributorRole> { inviteeRole },
                         replacedUserId: null,
                         rolesToReplace: new List<ContributorRole> { inviteeRole },
                         orgId: _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId),
                         status: InvitationStatus.Invited,
                         jobTitle: model.JobTitle,
                         invitedOrgName: newContributorDetailsModel?.InviteForFirstTime == true ? model.WorkingOrganisationName : newContributorDetailsModel?.WorkingOrgName
                     )
                 );

                if (string.IsNullOrWhiteSpace(invitationId))
                {
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later";
                    return RedirectToAction("CheckYourAnswers");
                }

                _logger.LogInformation("Successfully submitted new contributor details for email: {Email}", model.EmailAddress);
                var token = _invitationTokenService.GenerateToken(invitationId, model.EmailAddress);

                //send invitation email
                await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting new contributor details for email: {Email}", model.EmailAddress);
                TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later";
                return RedirectToAction("CheckYourAnswers");
            }


            _sessionHelper.SaveToSession<CheckYourAnswersViewModel>(HttpContext, SessionKeys.CheckYourAnswersContributorsModelSessionKey, model);
            return RedirectToAction("UserConfirmation");
        }

        [HttpGet]
        public IActionResult UserConfirmation()
        {
            ViewBag.whoDoYouWantToAdd = GetRole();
            return View();
        }
    }
}
