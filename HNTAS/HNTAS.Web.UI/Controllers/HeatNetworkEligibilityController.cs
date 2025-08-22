using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkEligibilityController : Controller
    {

        private readonly ISessionHelper _sessionHelper;

        public HeatNetworkEligibilityController(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }


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
            throw new Exception("Testing 500 error");
            //this.ShowBackButton("HowManyDwellingsIncluded", "HeatNetworkEligibility");
            //return View();
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
        public IActionResult YouAreEligible()
        {
            return View();
        }


        #endregion

        #region User Input pages

        [HttpGet]
        public IActionResult WhereIsTheHeatNetwork()
        {
            this.ShowBackButton("WhatDoYouWantToDo", "Home");
            var model = _sessionHelper.GetFromSession<WhereIsTheHeatNetworkViewModel>(HttpContext, SessionKeys.WhereIsTheHeatNetworkModelKey) ?? new WhereIsTheHeatNetworkViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WhereIsTheHeatNetwork(WhereIsTheHeatNetworkViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("WhatDoYouWantToDo", "Home");
                return View(model);
            }

            switch (model.PartOfTheUK)
            {
                case "england":
                case "scotland":
                case "wales":
                    _sessionHelper.SaveToSession<WhereIsTheHeatNetworkViewModel>(HttpContext, SessionKeys.WhereIsTheHeatNetworkModelKey, model);
                    return RedirectToAction("HowManyDwellingsIncluded", "HeatNetworkEligibility");
                case "ni":
                    _sessionHelper.SaveToSession<WhereIsTheHeatNetworkViewModel>(HttpContext, SessionKeys.WhereIsTheHeatNetworkModelKey, model);
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
            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedViewModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HowManyDwellingsIncluded(HowManyDwellingsIncludedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("WhereIsTheHeatNetwork", "HeatNetworkEligibility");
                return View(model);
            }
            switch (model.NumberOfDwellings)
            {
                case ">10":
                    _sessionHelper.SaveToSession<HowManyDwellingsIncludedViewModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
                    return RedirectToAction("IsHNCurrentlyOperating", "HeatNetworkEligibility");
                case "<10":
                    _sessionHelper.SaveToSession<HowManyDwellingsIncludedViewModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
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
            var model = _sessionHelper.GetFromSession<IsHNCurrentlyOperatingViewModel>(HttpContext, SessionKeys.IsHNCurrentlyOperatingModelKey) ?? new IsHNCurrentlyOperatingViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IsHNCurrentlyOperating(IsHNCurrentlyOperatingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("HowManyDwellingsIncluded", "HeatNetworkEligibility");
                return View(model);
            }
            switch (model.IsCurrentlyOperating)
            {
                case "yes":
                    _sessionHelper.SaveToSession<IsHNCurrentlyOperatingViewModel>(HttpContext, SessionKeys.IsHNCurrentlyOperatingModelKey, model);
                    return RedirectToAction("HNNotOperationalYet", "HeatNetworkEligibility");
                case "no":
                    _sessionHelper.SaveToSession<IsHNCurrentlyOperatingViewModel>(HttpContext, SessionKeys.IsHNCurrentlyOperatingModelKey, model);
                    return RedirectToAction("HaveYouSignedMEContract", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult HaveYouSignedMEContract()
        {
            this.ShowBackButton("IsHNCurrentlyOperating", "HeatNetworkEligibility");
            var model = _sessionHelper.GetFromSession<HaveYouSignedMEContractViewModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey) ?? new HaveYouSignedMEContractViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HaveYouSignedMEContract(HaveYouSignedMEContractViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("IsHNCurrentlyOperating", "HeatNetworkEligibility");
                return View(model);
            }
            switch (model.HaveYouSignedMEContract)
            {
                case "yes":
                    _sessionHelper.SaveToSession<HaveYouSignedMEContractViewModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model);
                    return RedirectToAction("MEContractIsSigned", "HeatNetworkEligibility");
                case "no":
                    _sessionHelper.SaveToSession<HaveYouSignedMEContractViewModel>(HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model);
                    return RedirectToAction("YouAreEligible", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }

        #endregion
    }
}