using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class RegistrationEligibilityController : Controller
    {

        private readonly ISessionHelper _sessionHelper;

        public RegistrationEligibilityController(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

               
        [HttpGet]
        public IActionResult AreYouTheRP()
        {
            this.ShowBackButton("WhatDoYouWantToDo", "Home");
            var model = _sessionHelper.GetFromSession<AreYouTheRPModel>(HttpContext, SessionKeys.AreYouTheRPModelKey) ?? new AreYouTheRPModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AreYouTheRP(AreYouTheRPModel model)
        {
            this.ShowBackButton("WhatDoYouWantToDo", "Home");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.AreYouTheRP)
            {
                case "yes":
                    _sessionHelper.SaveToSession<AreYouTheRPModel>(HttpContext, SessionKeys.AreYouTheRPModelKey, model);
                    return RedirectToAction("IsYourOrgWorkingOnANewHN");
                case "no":
                    _sessionHelper.SaveToSession<AreYouTheRPModel>(HttpContext, SessionKeys.AreYouTheRPModelKey, model);
                    return RedirectToAction("UserIsNotRP");
                default:
                    ModelState.AddModelError(nameof(model.AreYouTheRP), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult UserIsNotRP()
        {
            this.ShowBackButton("AreYouTheRP");
            return View();
        }

        [HttpGet]
        public IActionResult IsYourOrgWorkingOnANewHN()
        {
            this.ShowBackButton("AreYouTheRP");
            var model = _sessionHelper.GetFromSession<IsYourOrgWorkingOnANewHNModel>(HttpContext, SessionKeys.IsYourOrgWorkingOnANewHNModelKey) ?? new IsYourOrgWorkingOnANewHNModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IsYourOrgWorkingOnANewHN(IsYourOrgWorkingOnANewHNModel model)
        {
            this.ShowBackButton("AreYouTheRP");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.IsYourOrgWorkingOnANewHN)
            {
                case "yes":
                    _sessionHelper.SaveToSession<IsYourOrgWorkingOnANewHNModel>(HttpContext, SessionKeys.IsYourOrgWorkingOnANewHNModelKey, model);
                    return RedirectToAction("IsHNLocatedInEnglandScotlandWales");
                case "no":
                    _sessionHelper.SaveToSession<IsYourOrgWorkingOnANewHNModel>(HttpContext, SessionKeys.IsYourOrgWorkingOnANewHNModelKey, model);
                    return RedirectToAction("HnIsOperational");
                default:
                    ModelState.AddModelError(nameof(model.IsYourOrgWorkingOnANewHN), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult HnIsOperational()
        {
            this.ShowBackButton("IsYourOrgWorkingOnANewHN");
            return View();
        }

        [HttpGet]
        public IActionResult IsHNLocatedInEnglandScotlandWales()
        {
            this.ShowBackButton("IsYourOrgWorkingOnANewHN");
            var model = _sessionHelper.GetFromSession<IsHNLocatedInEnglandScotlandWalesModel>(HttpContext, SessionKeys.IsHNLocatedInEnglandScotlandWalesModelKey) ?? new IsHNLocatedInEnglandScotlandWalesModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IsHNLocatedInEnglandScotlandWales(IsHNLocatedInEnglandScotlandWalesModel model)
        {
            this.ShowBackButton("IsYourOrgWorkingOnANewHN");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.IsHNLocatedInEnglandScotlandWales)
            {
                case "yes":
                    _sessionHelper.SaveToSession<IsHNLocatedInEnglandScotlandWalesModel>(HttpContext, SessionKeys.IsHNLocatedInEnglandScotlandWalesModelKey, model);
                    return RedirectToAction("HowManyDwellingsIncluded");
                case "no":
                    _sessionHelper.SaveToSession<IsHNLocatedInEnglandScotlandWalesModel>(HttpContext, SessionKeys.IsHNLocatedInEnglandScotlandWalesModelKey, model);
                    return RedirectToAction("HnIsInNorthernIreland");
                default:
                    ModelState.AddModelError(nameof(model.IsHNLocatedInEnglandScotlandWales), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult HnIsInNorthernIreland()
        {
            this.ShowBackButton("IsHNLocatedInEnglandScotlandWales");
            return View();
        }

        [HttpGet]
        public IActionResult HowManyDwellingsIncluded()
        {
            this.ShowBackButton("IsHNLocatedInEnglandScotlandWales");
            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HowManyDwellingsIncluded(HowManyDwellingsIncludedModel model)
        {
            this.ShowBackButton("IsHNLocatedInEnglandScotlandWales");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            switch (model.HowManyDwellingsIncluded)
            {
                case "yes":
                    _sessionHelper.SaveToSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
                    return RedirectToAction("YouAreEligible");
                case "no":
                    _sessionHelper.SaveToSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
                    return RedirectToAction("LessThan6Dwellings");
                default:
                    ModelState.AddModelError(nameof(model.HowManyDwellingsIncluded), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult LessThan6Dwellings()
        {
            this.ShowBackButton("HowManyDwellingsIncluded");
            return View();
        }

        [HttpGet]
        public IActionResult YouAreEligible()
        {
            return View();
        }
    }
}