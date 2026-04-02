using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkLeads;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using Mono.TextTemplating;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Controllers
{
    public class NetworkLeadsController : Controller
    {
        private readonly ILogger<NetworkLeadsController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IInvitationService _invitationService;
        private readonly IUserService _userService;

        public NetworkLeadsController(ILogger<NetworkLeadsController> logger, ISessionHelper sessionHelper, IInvitationService invitationService, IUserService userService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _invitationService = invitationService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageLeads()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var networkLeads = await _userService.GetManagedUsers(userId, true);
            var networkLeadsToDisplay = networkLeads.Select(networkLead => new HNTAS.Web.UI.Models.User.UserDisplayModel
            {
                Id = networkLead.Id,
                Name = networkLead.Name,
                EmailAddress = networkLead.EmailId,
                Status = networkLead.Status.ToString(),
                IsCurrentUser = false
            }).ToList();
            var model = new ManageLeadsModel { NetworkLeads = networkLeadsToDisplay };
            return View(model);
        }

        [HttpGet]
        public IActionResult NewLeadDetails()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            var model = _sessionHelper.GetFromSession<NewLeadDetailsViewModel>(HttpContext, SessionKeys.NewLeadDetailsViewModelSessionKey) ?? new NewLeadDetailsViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> NewLeadDetails(NewLeadDetailsViewModel model)
        {
            this.ShowBackButton("ManageLeads");
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            try
            {
                var invitationId = await _invitationService.AddInvitedUserAsync(
                       userId,
                       new AddInvitationRequest(
                           emailAddress: model.EmailId,
                           firstName: model.FirstName,
                           lastName: model.LastName,
                           orgId: _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId),
                           contributorRoles: new List<ContributorRole> { ContributorRole.Coordinator },
                           status: InvitationStatus.Invited
                       )
                   );
            }
            catch(Exception ex)
            {
                _logger.LogError("An error occurred while processing your request. Please try again.");
                return View(model);
            }
            return RedirectToAction("NewLeadConfirmation");
        }

        [HttpGet]
        public IActionResult NewLeadConfirmation()
        {
            return View();
        }
    }
}
