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
                // TODO - if possible, refactor to use enum instead of string for HeatNetworkType to avoid magic strings and potential typos
                case "CommunalWithIntegralEC":
                    nextAction = "HeatNetworkCommunalECSummary";
                    break;
                case "CommunalWithSeparateUpstreamHN":
                    nextAction = "HeatNetworkCommunalNoECSummary";
                    break;
                case "DistrictWithOwnEC":
                case "DistrictWithSeparateUpstreamHN":
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
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey) ?? new HeatNetworkConnectionsViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult HeatNetworkConnections(HeatNetworkConnectionsViewModel model)
        {
            this.ShowBackButton("HeatNetworkType", "HeatNetworkRegistration");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            return RedirectToAction("HeatNetworkCommunalECSummary");
        }
    }
}
