using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetwork;
using Microsoft.AspNetCore.Mvc;
using HNTAS.Web.UI.Helpers;


namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {
        public void showBackButton(string action, string controller)
        {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action(action, controller);
        }

        [HttpGet]
        public IActionResult RunningAHN()
        {
            showBackButton("Guidance", "Guidance");
            return View(new RunningAHNViewModel());
        }

        [HttpPost]
        public IActionResult RunningAHN(RunningAHNViewModel model)
        {
            showBackButton("RunningAHN", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IsRunningHeatNetwork == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }

            return View("ServesGt10Dwellings", new ServesGt10DwellingsViewModel());
        }

        [HttpGet]
        public IActionResult ServesGt10Dwellings()
        {
            showBackButton("RunningAHN", "HeatNetwork");
            return View(new ServesGt10DwellingsViewModel());
        }

        [HttpPost]
        public IActionResult ServesGt10Dwellings(ServesGt10DwellingsViewModel model)
        {
            showBackButton("RunningAHN", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ServesMoreThan10Dwellings == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }

            return View("LocatedInUk", new LocatedInUkViewModel());
        }

        [HttpGet]
        public IActionResult LocatedInUk()
        {
            showBackButton("ServesGt10Dwellings", "HeatNetwork");
            return View(new LocatedInUkViewModel());
        }

        [HttpPost]
        public IActionResult LocatedInUk(LocatedInUkViewModel model)
        {
            showBackButton("ServesGt10Dwellings", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IsInUK == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }

            return View("OperatingAHN", new OperatingAHNViewModel());
        }

        [HttpGet]
        public IActionResult OperatingAHN()
        {
            showBackButton("LocatedInUk", "HeatNetwork");
            return View(new OperatingAHNViewModel());
        }

        [HttpPost]
        public IActionResult OperatingAHN(OperatingAHNViewModel model)
        {
            showBackButton("LocatedInUk", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IsExistingOrPlanned == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }

            // Eligible: show a message or redirect as needed
            ViewBag.ResultMessage = "You are eligible to register. Please create an account.";
            ViewBag.ShowCreateAccountButton = true;
            return View(model);
        }

        [HttpGet]
        public IActionResult EnterHNName()
        {
            // Utility.ShowBackButton(this, "CompanyConfirm", "Organisation");  point it to Heatwork , EnterWhat33WordsUrl when the page is ready
            return View(new HeatNetworkNameModel());
        }

        [HttpPost]
        public IActionResult EnterHNName(HeatNetworkNameModel model)
        {
            // Utility.ShowBackButton(this, "CompanyConfirm", "Organisation");  point it to Heatwork , EnterWhat33WordsUrl when the page is ready
            //if (string.IsNullOrWhiteSpace(model.hnName))
            //{
            //    ModelState.AddModelError("hnName", "Please enter the name of the Heat Network.");
            //} Add if needed, as required attribute is already in the model
                if (!ModelState.IsValid)
            {
                return View(model);
            }
            SessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, "HeatNetworkName", model);
            return RedirectToAction("EnterHNName", "HeatNetwork"); // add apropriate navigation
        }
    }
}
