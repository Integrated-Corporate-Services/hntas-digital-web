using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkEligibilityController : Controller
    {
        #region Model Keys

        private const string whereIsTheHeatNetworkModelKey = "whereIsTheHeatNetwork";
        private const string howManyDwellingsIncludedModelKey = "howManyDwellingsIncluded";
        private const string isHNCurrentlyOperatingModelKey = "isHNCurrentlyOperating";
        private const string doesElementExistModelKey = "doesElementExist";
        private const string hasElementBeenRegisteredModelKey = "hasElementBeenRegistered";
        private const string hasPlanningApplicationBeenSubmittedModelKey = "hasPlanningApplicationBeenSubmitted";
        private const string haveYouSignedMEContractModelKey = "haveYouSignedMEContract";

        #endregion

        #region Response Pages


        [HttpGet]
        public IActionResult LocatedInNorthernIreland()
        {
            this.ShowBackButton("WhereIsTheHeatNetwork", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult FewerThan10Dwellings()
        {
            this.ShowBackButton("HowManyDwellingsIncluded", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult HNNotOperationalYet()
        {
            this.ShowBackButton("IsHNCurrentlyOperating", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult MEContractIsSigned()
        {
            this.ShowBackButton("HaveYouSignedMEContract", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            this.ShowBackButton("DoesElementExist", "HeatNetworkEligibility");
            return View();
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            return View();
        }


        #endregion

        #region User Input pages

        [HttpGet]
        public IActionResult WhereIsTheHeatNetwork()
        {
            this.ShowBackButton("WhatDoYouWantToDo", "Home");
            var model = SessionHelper.GetFromSession<WhereIsTheHeatNetworkViewModel>(HttpContext, whereIsTheHeatNetworkModelKey) ?? new WhereIsTheHeatNetworkViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WhereIsTheHeatNetwork(WhereIsTheHeatNetworkViewModel model)
        {
            this.ShowBackButton("WhatDoYouWantToDo", "Home");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            switch(model.PartOfTheUK)
            {
                case "england":
                case "scotland":
                case "wales":
                    SessionHelper.SaveToSession<WhereIsTheHeatNetworkViewModel>(HttpContext, whereIsTheHeatNetworkModelKey, model);
                    return RedirectToAction("HowManyDwellingsIncluded", "HeatNetworkEligibility");
                case "ni":
                    SessionHelper.SaveToSession<WhereIsTheHeatNetworkViewModel>(HttpContext, whereIsTheHeatNetworkModelKey, model);
                    return RedirectToAction("LocatedInNorthernIreland", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
            
        }

        [HttpGet]
        public IActionResult HowManyDwellingsIncluded()
        {
            this.ShowBackButton("WhereIsTheHeatNetwork", "HeatNetworkEligibility");
            var model = SessionHelper.GetFromSession<HowManyDwellingsIncludedViewModel>(HttpContext, howManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HowManyDwellingsIncluded(HowManyDwellingsIncludedViewModel model)
        {
            this.ShowBackButton("WhereIsTheHeatNetwork", "HeatNetworkEligibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch(model.NumberOfDwellings)
            {
                case ">10":
                    SessionHelper.SaveToSession<HowManyDwellingsIncludedViewModel>(HttpContext, howManyDwellingsIncludedModelKey, model);
                    return RedirectToAction("IsHNCurrentlyOperating", "HeatNetworkEligibility");
                case "<10":
                    SessionHelper.SaveToSession<HowManyDwellingsIncludedViewModel>(HttpContext, howManyDwellingsIncludedModelKey, model);
                    return RedirectToAction("FewerThan10Dwellings", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult IsHNCurrentlyOperating()
        {
            this.ShowBackButton("HowManyDwellingsIncluded", "HeatNetworkEligibility");
            var model = SessionHelper.GetFromSession<IsHNCurrentlyOperatingViewModel>(HttpContext, isHNCurrentlyOperatingModelKey) ?? new IsHNCurrentlyOperatingViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IsHNCurrentlyOperating(IsHNCurrentlyOperatingViewModel model)
        {
            this.ShowBackButton("HowManyDwellingsIncluded", "HeatNetworkEligibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch(model.IsCurrentlyOperating)
            {
                case "yes":
                    SessionHelper.SaveToSession<IsHNCurrentlyOperatingViewModel>(HttpContext, isHNCurrentlyOperatingModelKey, model);
                    return RedirectToAction("HNNotOperationalYet", "HeatNetworkEligibility");
                case "no":
                    SessionHelper.SaveToSession<IsHNCurrentlyOperatingViewModel>(HttpContext, isHNCurrentlyOperatingModelKey, model);
                    return RedirectToAction("DoesElementExist", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult DoesElementExist()
        {
            this.ShowBackButton("IsHNCurrentlyOperating", "HeatNetworkEligibility");
            var model = SessionHelper.GetFromSession<DoesElementExistViewModel>(HttpContext, doesElementExistModelKey) ?? new DoesElementExistViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoesElementExist(DoesElementExistViewModel model)
        {
            this.ShowBackButton("IsHNCurrentlyOperating", "HeatNetworkEligibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch(model.DoesElementExist)
            {
                case "yes":
                    SessionHelper.SaveToSession<DoesElementExistViewModel>(HttpContext, doesElementExistModelKey, model);
                    return RedirectToAction("HasElementBeenRegistered", "HeatNetworkEligibility");
                case "no":
                    SessionHelper.SaveToSession<DoesElementExistViewModel>(HttpContext, doesElementExistModelKey, model);
                    return RedirectToAction("CheckYourAnswers", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult HasElementBeenRegistered()
        {
            this.ShowBackButton("DoesElementExist", "HeatNetworkEligibility");
            var model = SessionHelper.GetFromSession<HasElementBeenRegisteredViewModel>(HttpContext, hasElementBeenRegisteredModelKey) ?? new HasElementBeenRegisteredViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HasElementBeenRegistered(HasElementBeenRegisteredViewModel model)
        {
            this.ShowBackButton("DoesElementExist", "HeatNetworkEligibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch(model.HasElementBeenRegistered)
            {
                case "yes":
                    SessionHelper.SaveToSession<HasElementBeenRegisteredViewModel>(HttpContext, hasElementBeenRegisteredModelKey, model);
                    return RedirectToAction("HasPlanningApplicationBeenSubmitted", "HeatNetworkEligibility");
                case "no":
                    SessionHelper.SaveToSession<HasElementBeenRegisteredViewModel>(HttpContext, hasElementBeenRegisteredModelKey, model);
                    return RedirectToAction("CheckYourAnswers", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult HasPlanningApplicationBeenSubmitted()
        {
            this.ShowBackButton("HasElementBeenRegistered", "HeatNetworkEligibility");
            var model = SessionHelper.GetFromSession<HasPlanningApplicationBeenSubmittedViewModel>(HttpContext, hasPlanningApplicationBeenSubmittedModelKey) ?? new HasPlanningApplicationBeenSubmittedViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HasPlanningApplicationBeenSubmitted(HasPlanningApplicationBeenSubmittedViewModel model)
        {
            this.ShowBackButton("HasElementBeenRegistered", "HeatNetworkEligibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch(model.HasPlanningApplicationBeenSubmitted)
            {
                case "yes":
                    SessionHelper.SaveToSession<HasPlanningApplicationBeenSubmittedViewModel>(HttpContext, hasPlanningApplicationBeenSubmittedModelKey, model);
                    return RedirectToAction("HaveYouSignedMEContract", "HeatNetworkEligibility");
                case "no":
                    SessionHelper.SaveToSession<HasPlanningApplicationBeenSubmittedViewModel>(HttpContext, hasPlanningApplicationBeenSubmittedModelKey, model);
                    return RedirectToAction("CheckYourAnswers", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult HaveYouSignedMEContract()
        {
            this.ShowBackButton("HasPlanningApplicationBeenSubmitted", "HeatNetworkEligibility");
            var model = SessionHelper.GetFromSession<HaveYouSignedMEContractViewModel>(HttpContext, haveYouSignedMEContractModelKey) ?? new HaveYouSignedMEContractViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HaveYouSignedMEContract(HaveYouSignedMEContractViewModel model)
        {
            this.ShowBackButton("HasPlanningApplicationBeenSubmitted", "HeatNetworkEligibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch(model.HaveYouSignedMEContract)
            {
                case "yes":
                    SessionHelper.SaveToSession<HaveYouSignedMEContractViewModel>(HttpContext, haveYouSignedMEContractModelKey, model);
                    return RedirectToAction("MEContractIsSigned", "HeatNetworkEligibility");
                case "no":
                    SessionHelper.SaveToSession<HaveYouSignedMEContractViewModel>(HttpContext, haveYouSignedMEContractModelKey, model);
                    return RedirectToAction("CheckYourAnswers", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        #endregion
    }
}
