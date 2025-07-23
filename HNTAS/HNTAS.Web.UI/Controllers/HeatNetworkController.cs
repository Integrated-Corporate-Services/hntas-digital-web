using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
using Microsoft.AspNetCore.Mvc;


namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {
        //private const string heatNetworkLocationModelKey = "heatNetworkLocation";

        [HttpGet]
        public IActionResult EnterHNLocation() 
        {
            Utility.ShowBackButton(this, "Confirmation", "User"); // TODO - correct back page will be added after us-128
            var heatNetworkLocationModel = SessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            return View("EnterHNLocation", heatNetworkLocationModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnterHNLocation(HeatNetworkLocationModel model)
        {
            Utility.ShowBackButton(this, "Confirmation", "User"); // TODO - correct back page will be added after us-128

            if (string.IsNullOrWhiteSpace(model.HeatNetworkLocation))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkLocation), "Please enter the url.");
            }
            else if (!model.HeatNetworkLocation.Contains("https://what3words.com/"))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkLocation), "Invalid url. Please enter the correct url.");
            }
            else
            {
                // Extract the part after "https://what3words.com/"
                var prefix = "https://what3words.com/";
                var urlPart = model.HeatNetworkLocation.Substring(prefix.Length);

                // Validate: 3 words, joined by 2 dots, no whitespace
                // Regex: ^([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)$
                if (string.IsNullOrWhiteSpace(urlPart) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(urlPart, @"^([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)$"))
                {
                    ModelState.AddModelError(nameof(model.HeatNetworkLocation), "Invalid url. Please enter the correct url.");
                }
            }
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SessionHelper.SaveToSession<HeatNetworkLocationModel>(HttpContext, SessionHelper.SessionKeys.HeatNetworkLocationModelKey, model);

            return RedirectToAction("EnterHNName");
        }

        [HttpGet]
        public IActionResult EnterHNName()
        {
            Utility.ShowBackButton(this, "EnterHNLocation", "HeatNetwork");
            return View(new HeatNetworkNameModel());
        }

        [HttpPost]
        public IActionResult EnterHNName(HeatNetworkNameModel model)
        {
            Utility.ShowBackButton(this, "EnterHNLocation", "HeatNetwork");
            
            if(string.IsNullOrWhiteSpace(model.HeatNetworkName))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkName), "Please enter the name of the Heat Network.");
            }
            else if (model.HeatNetworkName.Length > 100)
            {
                ModelState.AddModelError(nameof(model.HeatNetworkName), "The heat network name cannot exceed 100 characters.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            SessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, "HeatNetworkName", model);
            return RedirectToAction("Confirmation"); // TODO - add apropriate navigation
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            //var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            //ViewBag.companyName = organisationModel?.CompanyDetails?.Title ?? ""; grab from user model, will find org name and contact details
            ViewBag.contactName = "John";
            ViewBag.unhid = "HDJ2123F";
            return View("Confirmation");
        }
    }
}
