using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddContributingOrganisation)]
    public class ExistingOrganisationController : Controller
    {
        private readonly IOrganisationService _organisationService;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;

        public ExistingOrganisationController(IOrganisationService organisationService, IUserService userService, ISessionHelper sessionHelper)
        {
            _organisationService = organisationService;
            _userService = userService;
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        public IActionResult AddOrRegister()
        {
            var organisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            if (organisationName != null)
            {
                this.ShowBackButton("UserAccount", "Dashboard");
            }
            _sessionHelper.SaveToSession<bool>(HttpContext, SessionKeys.IsAddOrganisationDetailsNonRPJourneySessionKey, true);
            return View("AddOrRegister");
        }

        [HttpGet]
        public IActionResult Search()
        {
            this.ShowBackButton("AddOrRegister");

            return View("Search", new OrganisationSearchViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Search(OrganisationSearchViewModel searchViewModel)
        {
            this.ShowBackButton("AddOrRegister");

            if (string.IsNullOrWhiteSpace(searchViewModel.SearchTerm))
            {
                return View("Search", searchViewModel);
            }

            //call the api to search organisation by name or id
            var organisation = await _organisationService.GetOrganisationByIdOrName(searchViewModel.SearchTerm.Trim());

            if (organisation == null)
            {
                ModelState.AddModelError(nameof(searchViewModel.SearchTerm), "No organisation found with the provided ID or Name. Please check and try again.");
                return View("Search", searchViewModel);
            }


            _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationId, organisation?.OrgId);

            return RedirectToAction("ConfirmOrganisation");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmOrganisation()
        {
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            if (string.IsNullOrWhiteSpace(orgId))
            {
                return BadRequest("Organisation id not found.");
            }

            this.ShowBackButton("Search");

            ViewBag.ChangeUrl = ViewBag.BackLinkUrl;

            var organisation = await _organisationService.GetOrganisationById(orgId.ToUpper());

            var model = new ConfirmOrganisationViewModel
            {
                Title = organisation.Name,
                OrganisationId = organisation.OrgId,
                RegisteredOfficeAddress = new RegisteredOfficeAddressModel
                {
                    AddressLine1 = organisation.RegisteredAddress?.AddressLine1,
                    AddressLine2 = organisation.RegisteredAddress?.AddressLine2,
                    Locality = organisation.RegisteredAddress?.Town,
                    Country = organisation.RegisteredAddress?.County,
                    PostalCode = organisation.RegisteredAddress?.Postcode
                }
            };

            return View("ConfirmOrganisation", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfirmOrganisation()
        {
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            if (string.IsNullOrWhiteSpace(orgId))
            {
                return BadRequest("Organisation id not found.");
            }

            await _userService.UpdateUserWithExistingOrganisationId(userId, orgId);

            TempData["Confirmation_Organisation_Id"] = orgId;

            _sessionHelper.ClearAllFlowRelatedSessionData(HttpContext);

            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            var orgId = TempData["Confirmation_Organisation_Id"] as string;
            ViewBag.OrganisationId = orgId;
            ViewBag.ShowBackButton = false;
            return View("Organisation/Confirmation");
        }
    }
}
