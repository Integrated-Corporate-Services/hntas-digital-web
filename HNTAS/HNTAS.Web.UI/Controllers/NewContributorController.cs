using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PreferredContactType = HNTAS.Web.UI.Models.Enums.PreferredContactType;


namespace HNTAS.Web.UI.Controllers
{
    public class NewContributorController : Controller
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly ILogger<NewContributorController> _logger;
        private readonly ISessionHelper _sessionHelper;

        public NewContributorController(ILogger<NewContributorController> logger, IWorkflowManager workflowManager, ISessionHelper sessionHelper)
        {
            _logger = logger;
            _workflowManager = workflowManager;
            _sessionHelper = sessionHelper;
        }


        [HttpGet]
        public IActionResult AddEmailAddress()
        {
            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();

            this.ShowBackButton("AddContributor", "UserManagement");

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            return View(state.Data.AddUserEmailAddressModel ?? new AddUserEmailAddressModel());
        }

        [HttpPost]
        public IActionResult SaveEmailAddress(AddUserEmailAddressModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("AddContributor", "UserManagement");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("AddEmailAddress", model);
            }

            // Logic to save email address goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
                m => m.AddUserEmailAddressModel = model,
                ContributorWorkflowStep.ContactDetails
            );

            return RedirectToAction("ContactDetails");
        }

        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.ContactDetails)]
        public IActionResult ContactDetails()
        {
            var state = _workflowManager.GetState<AddNewContributorWorkflowModel>();
            this.ShowBackButton("AddEmailAddress");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(state.Data.ContributorContactDetailsModel ?? new ContributorContactDetailsModel());
        }

        [HttpPost]
        public IActionResult SaveContactDetails(ContributorContactDetailsModel contactDetails)
        {

            switch (contactDetails.PreferredContactType)
            {
                case PreferredContactType.Landline:
                    contactDetails.MobileNumber = null;
                    ModelState.Remove(nameof(contactDetails.MobileNumber));
                    if (string.IsNullOrWhiteSpace(contactDetails.LandlineNumber))
                        ModelState.AddModelError(nameof(contactDetails.LandlineNumber), "Enter your landline number.");
                    break;
                case PreferredContactType.Mobile:
                    contactDetails.LandlineNumber = null;
                    contactDetails.ContactNumberExtension = null;
                    ModelState.Remove(nameof(contactDetails.LandlineNumber));
                    ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                    if (string.IsNullOrWhiteSpace(contactDetails.MobileNumber))
                        ModelState.AddModelError(nameof(contactDetails.MobileNumber), "Enter your mobile number.");
                    break;
                default:
                    contactDetails.LandlineNumber = null;
                    contactDetails.ContactNumberExtension = null;
                    contactDetails.MobileNumber = null;
                    ModelState.Remove(nameof(contactDetails.LandlineNumber));
                    ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                    ModelState.Remove(nameof(contactDetails.MobileNumber));
                    break;
            }

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ContactDetails");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("ContactDetails", contactDetails);
            }
            // Logic to save contact details goes here

            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
                m => m.ContributorContactDetailsModel = contactDetails,
                ContributorWorkflowStep.ChooseHeatNetwork
            );

            return RedirectToAction("ChooseHeatNetwork");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.ChooseHeatNetwork)]
        public IActionResult ChooseHeatNetwork()
        {
            var model = new HeatNetworkInformationModel
            {
                HeatNetworks =
                [
                    new SelectListItem { Value = "1", Text = "Heat Network A" },
                    new SelectListItem { Value = "2", Text = "Heat Network B" },
                    new SelectListItem { Value = "3", Text = "Heat Network C" }
                ]
            };

            this.ShowBackButton("ContactDetails");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveChosenHeatNetwork(HeatNetworkInformationModel model)
        {
            model.HeatNetworks = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Heat Network A" },
                new SelectListItem { Value = "2", Text = "Heat Network B" },
                new SelectListItem { Value = "3", Text = "Heat Network C" }
            };

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ChooseHeatNetwork");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("ChooseHeatNetwork", model);
            }

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
             m => m.HeatNetworkInformationModel = model,
             ContributorWorkflowStep.ChooseRole
            );

            return RedirectToAction("ChooseRole");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.ChooseRole)]
        public IActionResult ChooseRole()
        {
            var model = new SelectRoleModel();

            // Populate the list with the roles you specified.
            model.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Designated Designers" },
                new SelectListItem { Value = "2", Text = "Designated Contractors" },
                new SelectListItem { Value = "3", Text = "Designated Operators" }
            };

            this.ShowBackButton("ChooseHeatNetwork");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveChosenRole(SelectRoleModel model)
        {
            // Re-populate the list in case of a validation error.
            model.Roles =
            [
                new SelectListItem { Value = "1", Text = "Designated Designers" },
                new SelectListItem { Value = "2", Text = "Designated Contractors" },
                new SelectListItem { Value = "3", Text = "Designated Operators" }
            ];

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ChooseRole");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("ChooseRole", model);
            }

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
             m => m.SelectRoleModel = model,
             ContributorWorkflowStep.Review
            );

            return RedirectToAction("CheckYourAnswers");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.Review)]
        public IActionResult CheckYourAnswers()
        {
            this.ShowBackButton("ChooseRole");
            return View();
        }

        [HttpPost]
        public IActionResult SubmitAnswers()
        {

            TempData["FullName"] = "Test Name";
            TempData["HeatNetwork"] = "Test heatNetwork";
            TempData["CompanyName"] = "ABC company";

            // Logic to save details goes here
            _workflowManager.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.Confirmation);

            return RedirectToAction("Confirmation");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(ContributorWorkflowStep.Confirmation)]
        public IActionResult Confirmation()
        {

            // Retrieve the data from TempData.
            var fullName = TempData["FullName"] as string;
            var heatNetwork = TempData["HeatNetwork"] as string;
            var companyName = TempData["CompanyName"] as string;

            // You can use a ViewBag or ViewData to pass the data to the view.
            ViewData["FullName"] = fullName;
            ViewData["HeatNetwork"] = heatNetwork;
            ViewData["CompanyName"] = companyName;

            return View();

        }
    }
}
