using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkRegistrationController : Controller
    {
        private readonly ISessionHelper _sessionHelper;

        public HeatNetworkRegistrationController(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        #region wip
        [HttpGet]
        public IActionResult HeatNetworkDwellingsCheck()
        {
            this.ShowBackButton("HeatNetworkAsync", "UserManagement");
            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkDwellingsCheck(HowManyDwellingsIncludedModel model)
        {
            this.ShowBackButton("HeatNetworkAsync", "UserManagement");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
            switch (model.HowManyDwellingsIncluded)
            {
                case "yes":
                    return RedirectToAction("HeatNetworkIntroduction");
                case "no":
                    return RedirectToAction("SixOrMoreDwellingsAnswerNo");
                default:
                    ModelState.AddModelError(nameof(model.HowManyDwellingsIncluded), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult SixOrMoreDwellingsAnswerNo()
        {
            this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkIntroduction()
        {
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkType()
        {
            this.ShowBackButton("HeatNetworkIntroduction", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey) ?? new HeatNetworkTypeViewModel();
            return View(model);
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkType(HeatNetworkTypeViewModel model)
        {
            this.ShowBackButton("HeatNetworkIntroduction", "HeatNetworkRegistration");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string nextAction;
            switch (model.HeatNetworkType)
            {
                case Models.Enums.HeatNetworkType.CommunalWithIntegralEC:
                    nextAction = "HeatNetworkCommunalECSummary";
                    break;
                case Models.Enums.HeatNetworkType.CommunalWithSeparateUpstreamHN:
                    nextAction = "HeatNetworkCommunalNoECSummary";
                    break;
                case Models.Enums.HeatNetworkType.DistrictWithOwnEC:
                case Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN:
                    nextAction = "HeatNetworkConnections";
                    break;
                default:
                    ModelState.AddModelError(nameof(model.HeatNetworkType), "Please select a valid option.");
                    return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey, model);
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalECSummary()
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalNoECSummary()
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            return View();
        }

        [HttpGet]
        public IActionResult HeatNetworkConnections()
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            var newModel = new HeatNetworkConnectionsViewModel();
            if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithOwnEC)
            {
                ViewBag.IsDistrictWithOwnEC = true;
                newModel.IsUpstreamDistrictHeatNetworkConnections = false;
            }
            else if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithSeparateUpstreamHN)
            {
                ViewBag.IsDistrictWithOwnEC = false;
                newModel.IsDownstreamDistrictHeatNetworkConnections = false;
            }
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey) ?? newModel;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkConnections(HeatNetworkConnectionsViewModel model)
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");

            if (model.IsCommunalBuilding && !model.NoOfCommunalBuilding.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfCommunalBuilding), "Enter the number of communal buildings");
            }

            if (model.IsDomesticConsumer && !model.NoOfDomesticConsumer.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfDomesticConsumer), "Enter the number of domestic consumers");
            }

            if (model.IsNonDomesticConsumer && !model.NoOfNonDomesticConsumer.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfNonDomesticConsumer), "Enter the number of non-domestic consumers");
            }

            if (model.IsDownstreamDistrictHeatNetworkConnections && !model.NoOfDownstreamDistrictHeatNetworkConnections.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfDownstreamDistrictHeatNetworkConnections), "Enter the number of district connections");
            }

            if (model.IsUpstreamDistrictHeatNetworkConnections && !model.NoOfUpstreamDistrictHeatNetworkConnections.HasValue)
            {
                ModelState.AddModelError(nameof(model.NoOfUpstreamDistrictHeatNetworkConnections), "Enter the number of district connections");
            }

            if (!model.IsCommunalBuilding && !model.IsDomesticConsumer && !model.IsNonDomesticConsumer && !model.IsUpstreamDistrictHeatNetworkConnections && !model.IsDownstreamDistrictHeatNetworkConnections)
            {
                // Model-level error (not associated with a specific property)
                ModelState.AddModelError("HeatNetworkConnections", "Select at least one connection type");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey, model);
            var hnTypeModel = _sessionHelper.GetFromSession<HeatNetworkTypeViewModel>(HttpContext, SessionKeys.HeatNetworkTypeViewModelKey);
            if (hnTypeModel.HeatNetworkType == Models.Enums.HeatNetworkType.DistrictWithOwnEC)
            {
                return RedirectToAction("HeatNetworkDistrictEcSummary");
            }
            else
            {
                return RedirectToAction("HeatNetworkDistrictNoEcSummary");
            }
        }

        public IActionResult HeatNetworkDistrictEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View(model);
        }

        public IActionResult HeatNetworkDistrictNoEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View(model);
        }

        [HttpGet]
        public IActionResult HeatNetworkSummary()
        {
            return RedirectToAction("EnterHnName", "HeatNetwork");
        }
    }
}
