using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class OrganisationController : Controller
    {
        private readonly ICompaniesHouseService _companiesHouseService;
        private readonly ILogger<OrganisationController> _logger;
        private readonly IUserService _userService;

        public OrganisationController(ICompaniesHouseService companiesHouseService, ILogger<OrganisationController> logger, IUserService userService)
        {
            _companiesHouseService = companiesHouseService;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Start()
        {
            SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, false);
            return RedirectToAction("Type");
        }

        [HttpGet]
        public IActionResult Type()
        {
            var model = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey) ?? new OrganisationModel();
            model.OrganisationTypes = GetOrganisationTypeOptions();

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = isCheckAnswerFlow;
            ViewBag.BackLinkUrl = isCheckAnswerFlow ? Url.Action("CheckAnswers", "User") : null;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Type(OrganisationModel model)
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            if (orgModel != null)
            {
                model.CompanyNumber = orgModel.CompanyNumber;
                model.CompanyDetails = orgModel.CompanyDetails;
            }
        
            ModelState.Remove(nameof(model.CompanyNumber));
            ModelState.Remove(nameof(model.CompanyDetails));

            if (ModelState.IsValid)
            {
                string? selectedOrganisationTypeText = GetOrganisationTypeOptions()
                    .FirstOrDefault(item => item.Value == model.SelectedOrganisationType)?.Text;

                if(selectedOrganisationTypeText == null)
                {
                    ModelState.AddModelError(nameof(model.SelectedOrganisationType), "Please select a valid organisation type.");
                    model.OrganisationTypes = GetOrganisationTypeOptions();
                    return View("Type", model);
                }

                model.SelectedOrganisationTypeText = selectedOrganisationTypeText;

                if (model.SelectedOrganisationType == OrganisationType.OtherUkOrganisation.ToString() || model.SelectedOrganisationType == OrganisationType.OverseasOrganisation.ToString())
                {
                    ModelState.AddModelError(nameof(model.SelectedOrganisationType), "Not applicable for this scope");
                    model.OrganisationTypes = GetOrganisationTypeOptions();
                    return View("Type", model);
                }

                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey, model);

                bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
                if (isCheckAnswerFlow)
                    return RedirectToAction("CheckAnswers", "User");

                return RedirectToAction("CompanyNumber");
            }

            model.OrganisationTypes = GetOrganisationTypeOptions();
            return View("Type", model);
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CompanyNumber()
        {
            var model = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = isCheckAnswerFlow
                ? Url.Action("CheckAnswers", "User")
                : Url.Action("Type", "Organisation");

            return View("CompanyNumber", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public async Task<IActionResult> CompanyNumberAsync(OrganisationModel model)
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            orgModel.CompanyNumber = model.CompanyNumber;
            ModelState.Clear();
            TryValidateModel(orgModel);
            ModelState.Remove(nameof(orgModel.SelectedOrganisationType));

            CompanyDetailsModel? companyDetails = null;

            if (ModelState.IsValid)
            {
                try
                {
                    companyDetails = await _companiesHouseService.GetCompanyByNumberAsync(orgModel.CompanyNumber);
                    if (companyDetails == null)
                        ModelState.AddModelError(nameof(orgModel.CompanyNumber), "Company number not found. Please check and try again.");
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(nameof(orgModel.CompanyNumber), "Could not verify company number at this time. Please try again later.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(orgModel.CompanyNumber), "An unexpected error occurred during company number verification.");
                }
            }

            if (ModelState.IsValid && companyDetails != null)
            {
                orgModel.CompanyDetails = companyDetails;
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey, orgModel);
                return RedirectToAction("CompanyConfirm");
            }

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = isCheckAnswerFlow
                ? Url.Action("CheckAnswers", "User")
                : Url.Action("Type", "Organisation");

            return View("CompanyNumber", orgModel);
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CompanyConfirm()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            if (organisationModel?.CompanyDetails == null)
                return RedirectToAction("CompanyNumber");

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = isCheckAnswerFlow
                ? Url.Action("CheckAnswers", "User")
                : Url.Action("CompanyNumber", "Organisation");

            return View("CompanyConfirm", organisationModel.CompanyDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public async Task<IActionResult> ConfirmAndContinue()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            if (organisationModel?.CompanyDetails == null)
                return RedirectToAction("CompanyNumber");

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            if (isCheckAnswerFlow)
                return RedirectToAction("CheckAnswers", "User");

            //Set empty contact details to ensure they pass the EnsureSessionForOrganisationFlowOnPost action filter validation 
            //on user controller actions
            bool? alreadyExists = await _userService.IsOrganisationHasRpUser(organisationModel?.CompanyNumber);
            if(alreadyExists.HasValue && alreadyExists.Value)
            { 
                return RedirectToAction("AlreadyRegistered");
            }

            var existingUserModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            if(existingUserModel == null)
            {
                existingUserModel = new UserModel();
                SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, existingUserModel);
            }

            return RedirectToAction("ConfirmRPIsRC", "User");
        }

        public IActionResult AlreadyRegistered()
        {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyConfirm", "Organisation");
            return View();
        }

        private List<SelectListItem> GetOrganisationTypeOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = OrganisationType.UkCompaniesHouse.ToString(), Text = "UK company registered with Companies House" },
                new SelectListItem { Value = OrganisationType.OtherUkOrganisation.ToString(), Text = "Other UK organisation" },
                new SelectListItem { Value = OrganisationType.OverseasOrganisation.ToString(), Text = "Overseas organisation" }
            };
        }
    }
}