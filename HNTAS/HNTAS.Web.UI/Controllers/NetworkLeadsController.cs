using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkLeads;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class NetworkLeadsController : Controller
    {
        private readonly ILogger<NetworkLeadsController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IInvitationService _invitationService;
        private readonly IUserService _userService;
        private readonly IInvitationTokenService _iInvitationTokenService;

        public NetworkLeadsController(ILogger<NetworkLeadsController> logger, ISessionHelper sessionHelper, IInvitationService invitationService, IUserService userService, IInvitationTokenService invitationTokenService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _invitationService = invitationService;
            _userService = userService;
            _iInvitationTokenService = invitationTokenService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageLeads()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var networkLeads = await _userService.GetNetworkLeads(userId);
            var networkLeadsToDisplay = networkLeads.Select(networkLead => new UserDisplayModel
            {
                Name = networkLead.FullName,
                EmailAddress = networkLead.Email,
                Status = networkLead.Status.ToString(),
                IsCurrentUser = false
            }).ToList();
            var model = new ManageLeadsModel { NetworkLeads = networkLeadsToDisplay };
            return View(model);
        }

        public ActionResult Start()
        {
            //clear session data for new lead invitation
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewLeadDetailsViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SameOrganisationViewModelSessionKey);

            return RedirectToAction(nameof(InviteLeadByEmail));
        }

        [HttpGet]
        public IActionResult InviteLeadByEmail()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            var model = _sessionHelper.GetFromSession<InviteLeadViewModel>(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey)
                        ?? new InviteLeadViewModel();

            this.ShowBackButton(nameof(ManageLeads));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLeadEmailAsync(InviteLeadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton(nameof(ManageLeads));
                return View(nameof(InviteLeadByEmail), model);
            }

            //Check if the email address is already active in the system
            var existingUser = await _userService.GetUserByEmailIdAsync(model.EmailAddress);
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            //check if the user is rp
            if (existingUser != null && existingUser.Roles.Contains(UserRole.ResponsibleParty))
            {
                ModelState.AddModelError("EmailAddress", "This user cannot be added as a Network Manager");
                return View(nameof(InviteLeadByEmail), model);
            }
            else if (existingUser != null && existingUser.Roles.Contains(UserRole.NetworkManager) && existingUser.ContributingOrganisations!.Contains(orgId!))
            {
                //check if the user is already a network lead for this organisation
                ModelState.AddModelError("EmailAddress", "This user is already a Network Manager for this organisation");
                return View(nameof(InviteLeadByEmail), model);
            }
            else if (existingUser != null)
            {
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey, model);
                //redirect to profile found page
                return RedirectToAction("NewLeadAlreadyExists");
            }

            // Save the invited email to session or proceed to next flow
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey, model);

            return RedirectToAction(nameof(NewLeadDetails));
        }

        [HttpGet]
        public async Task<IActionResult> NewLeadAlreadyExistsAsync()
        {
            // Retrieve existing profile match details from session/database lookup
            var emailModel = _sessionHelper.GetFromSession<InviteLeadViewModel>(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey);
            var organisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            if (emailModel == null)
            {
                TempData["ErrorMessage"] = "There was an error retrieving the existing user details. Please try again later.";
                return RedirectToAction(nameof(ManageLeads));
            }
            ;

            var existingUser = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);

            var viewModel = new NewLeadAlreadyExistsViewModel
            {
                OrganisationName = organisationName,
                FirstName = existingUser?.FirstName,
                LastName = existingUser?.LastName,
                EmailAddress = existingUser?.EmailId,
                JobTitle = existingUser?.JobTitle
            };

            this.ShowBackButton(nameof(InviteLeadByEmail));

            return View("NewLeadAlreadyExists", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNewLeadAlreadyExistsAsync(NewLeadAlreadyExistsViewModel model)
        {
            // Repopulate metadata for redisplay if validation fails
            model.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            var emailModel = _sessionHelper.GetFromSession<InviteLeadViewModel>(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey);

            if (!ModelState.IsValid)
            {
                var existingUser = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);

                model.FirstName = existingUser?.FirstName;
                model.LastName = existingUser?.LastName;
                model.EmailAddress = existingUser?.EmailId;
                model.JobTitle = existingUser?.JobTitle;

                this.ShowBackButton(nameof(InviteLeadByEmail));
                return View(nameof(NewLeadAlreadyExistsAsync), model);
            }

            if (model.IsCorrectPerson == true)
            {
                //fill new lead details view model with existing user details and save to session
                // check for same organisation and redirect accordingly
                var existingUser = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);

                var newLeadDetailsModel = new NewLeadDetailsViewModel
                {
                    FirstName = existingUser?.FirstName,
                    LastName = existingUser?.LastName,
                    JobTitle = existingUser?.JobTitle,
                    InviteForFirstTime = false
                };

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewLeadDetailsViewModelSessionKey, newLeadDetailsModel);

                return RedirectToAction(nameof(CheckYourAnswers));
            }
            else
            {
                // Redirect back to enter a different email address
                return RedirectToAction(nameof(InviteLeadByEmail));
            }
        }



        [HttpGet]
        public IActionResult NewLeadDetails()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            var model = _sessionHelper.GetFromSession<NewLeadDetailsViewModel>(HttpContext, SessionKeys.NewLeadDetailsViewModelSessionKey) ?? new NewLeadDetailsViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewLeadDetails(NewLeadDetailsViewModel model)
        {
            this.ShowBackButton(nameof(ManageLeads) );

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var emailModel = _sessionHelper.GetFromSession<InviteLeadViewModel>(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey);

            if (emailModel == null)
            {
                TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later";
                return RedirectToAction(nameof(ManageLeads));
            }

            model.InviteForFirstTime = true;

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NewLeadDetailsViewModelSessionKey, model);

            return RedirectToAction(nameof(SameOrganisation));
        }

        [HttpGet]
        public IActionResult SameOrganisation()
        {
            var viewModel = _sessionHelper.GetFromSession<SameOrganisationViewModel>(
                HttpContext,
                SessionKeys.SameOrganisationViewModelSessionKey) ?? new SameOrganisationViewModel();

            this.ShowBackButton(nameof(NewLeadAlreadyExistsAsync));

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSameOrganisationAsync(SameOrganisationViewModel model)
        {
            ModelState.Clear();

            if (model.WorksForSameOrganisation == false &&
                string.IsNullOrWhiteSpace(model.OrganisationName))
            {
                ModelState.AddModelError(
                    nameof(model.OrganisationName),
                    "Enter the organisation name");
            }

            if (!ModelState.IsValid)
            {
                this.ShowBackButton(nameof(NewLeadAlreadyExistsAsync));
                return View(nameof(SameOrganisation), model);
            }

            if (model.WorksForSameOrganisation == true)
            {
                model.OrganisationName = null;
            }

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SameOrganisationViewModelSessionKey, model);

           return RedirectToAction(nameof(CheckYourAnswers));
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            var leadDetails = _sessionHelper.GetFromSession<NewLeadDetailsViewModel>(
                  HttpContext,
                  SessionKeys.NewLeadDetailsViewModelSessionKey);

            if (leadDetails?.InviteForFirstTime == true)
            {
                this.ShowBackButton(nameof(SameOrganisation));
            }
            else
            {
                this.ShowBackButton("NewLeadAlreadyExists");
            }

            return View(CreateCYAModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckYourAnswers(CheckYourAnswersViewModel model)
        {
            model = CreateCYAModel();

            if (!ModelState.IsValid)
            {
                var leadDetails = _sessionHelper.GetFromSession<NewLeadDetailsViewModel>(
                HttpContext,
                SessionKeys.NewLeadDetailsViewModelSessionKey);

                this.ShowBackButton(leadDetails?.InviteForFirstTime == true 
                    ? nameof(SameOrganisation) : "NewLeadAlreadyExists");

                return View(model);
            }

            try
            {
                var userId = _sessionHelper.GetFromSession<string>(
                    HttpContext,
                    SessionKeys.UserModel_Id_SessionKey);

                var emailModel = _sessionHelper.GetFromSession<InviteLeadViewModel>(
                    HttpContext,
                    SessionKeys.NewLeadInviteEmailViewModelSessionKey);

                var leadDetails = _sessionHelper.GetFromSession<NewLeadDetailsViewModel>(
                    HttpContext,
                    SessionKeys.NewLeadDetailsViewModelSessionKey);

                var orgId = _sessionHelper.GetFromSession<string>(
                    HttpContext,
                    SessionKeys.OrganisationId);

                var invitee = await _userService.GetUserByEmailIdAsync(emailModel?.EmailAddress);


                var invitationId = await _invitationService.AddInvitedUserAsync(
                    userId,
                        new AddInvitationRequest(
                            emailAddress: emailModel?.EmailAddress,
                            firstName: leadDetails.FirstName,
                            lastName: leadDetails.LastName,
                            orgId: orgId,
                            contributorRoles: new List<ContributorRole> { ContributorRole.NetworkManager },
                            status: InvitationStatus.Invited,
                            jobTitle: leadDetails.JobTitle,
                            invitedOrgName: model.WorkingOrganisationName
                        )
                    );

                if (string.IsNullOrWhiteSpace(invitationId))
                {
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later";

                    return RedirectToAction(nameof(ManageLeads));
                }

                _logger.LogInformation("Successfully submitted new organisation user details.");

                var token = _iInvitationTokenService.GenerateToken(invitationId, emailModel?.EmailAddress);

                await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));

                return RedirectToAction(nameof(NewLeadConfirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the network manager invitation.");

                return View(nameof(CheckYourAnswers), model);
            }
        }


        [HttpGet]
        public IActionResult NewLeadConfirmation()
        {
            //clear session data for new lead invitation
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewLeadInviteEmailViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NewLeadDetailsViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SameOrganisationViewModelSessionKey);

            return View();
        }

        private CheckYourAnswersViewModel CreateCYAModel()
        {
            var leadDetails = _sessionHelper.GetFromSession<NewLeadDetailsViewModel>(
                HttpContext,
                SessionKeys.NewLeadDetailsViewModelSessionKey);
            var sameOrgDetails = _sessionHelper.GetFromSession<SameOrganisationViewModel>(
                HttpContext,
                SessionKeys.SameOrganisationViewModelSessionKey);
            var emailModel = _sessionHelper.GetFromSession<InviteLeadViewModel>(
                HttpContext,
                SessionKeys.NewLeadInviteEmailViewModelSessionKey);

            return new CheckYourAnswersViewModel
            {
                FirstName = leadDetails?.FirstName,
                LastName = leadDetails?.LastName,
                EmailAddress = emailModel?.EmailAddress,
                JobTitle = leadDetails?.JobTitle,
                WorkForSameOrganisation = sameOrgDetails?.WorksForSameOrganisation,
                WorkingOrganisationName = sameOrgDetails?.OrganisationName,
                InviteForFirstTime = leadDetails?.InviteForFirstTime ?? false,
                InvitedRole = "Network Manager"
            };
        }
    }
}
