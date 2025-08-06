using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkEligibilityController : Controller
    {
    
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
            var model = SessionHelper.GetFromSession<WhereIsTheHeatNetworkViewModel>(HttpContext, SessionHelper.SessionKeys.WhereIsTheHeatNetworkModelKey) ?? new WhereIsTheHeatNetworkViewModel();
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
                    SessionHelper.SaveToSession<WhereIsTheHeatNetworkViewModel>(HttpContext, SessionHelper.SessionKeys.WhereIsTheHeatNetworkModelKey, model);
                    return RedirectToAction("HowManyDwellingsIncluded", "HeatNetworkEligibility");
                case "ni":
                    SessionHelper.SaveToSession<WhereIsTheHeatNetworkViewModel>(HttpContext, SessionHelper.SessionKeys.WhereIsTheHeatNetworkModelKey, model);
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
            var model = SessionHelper.GetFromSession<HowManyDwellingsIncludedViewModel>(HttpContext, SessionHelper.SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedViewModel();
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
                    SessionHelper.SaveToSession<HowManyDwellingsIncludedViewModel>(HttpContext, SessionHelper.SessionKeys.HowManyDwellingsIncludedModelKey, model);
                    return RedirectToAction("IsHNCurrentlyOperating", "HeatNetworkEligibility");
                case "<10":
                    SessionHelper.SaveToSession<HowManyDwellingsIncludedViewModel>(HttpContext, SessionHelper.SessionKeys.HowManyDwellingsIncludedModelKey, model);
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
            var model = SessionHelper.GetFromSession<IsHNCurrentlyOperatingViewModel>(HttpContext, SessionHelper.SessionKeys.IsHNCurrentlyOperatingModelKey) ?? new IsHNCurrentlyOperatingViewModel();
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
                    SessionHelper.SaveToSession<IsHNCurrentlyOperatingViewModel>(HttpContext, SessionHelper.SessionKeys.IsHNCurrentlyOperatingModelKey, model);
                    return RedirectToAction("HNNotOperationalYet", "HeatNetworkEligibility");
                case "no":
                    SessionHelper.SaveToSession<IsHNCurrentlyOperatingViewModel>(HttpContext, SessionHelper.SessionKeys.IsHNCurrentlyOperatingModelKey, model);
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
            var model = SessionHelper.GetFromSession<HaveYouSignedMEContractViewModel>(HttpContext, SessionHelper.SessionKeys.HaveYouSignedMEContractModelKey) ?? new HaveYouSignedMEContractViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HaveYouSignedMEContract(HaveYouSignedMEContractViewModel model)
        {
            this.ShowBackButton("IsHNCurrentlyOperating", "HeatNetworkEligibility");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.HaveYouSignedMEContract)
            {
                case "yes":
                    SessionHelper.SaveToSession<HaveYouSignedMEContractViewModel>(HttpContext, SessionHelper.SessionKeys.HaveYouSignedMEContractModelKey, model);
                    return RedirectToAction("MEContractIsSigned", "HeatNetworkEligibility");
                case "no":
                    SessionHelper.SaveToSession<HaveYouSignedMEContractViewModel>(HttpContext, SessionHelper.SessionKeys.HaveYouSignedMEContractModelKey, model);
                    return RedirectToAction("YouAreEligible", "HeatNetworkEligibility");
                default:
                    ModelState.AddModelError(string.Empty, "Please select a valid option.");
                    return View(model);
            }
        }
     
        #endregion
    }
}