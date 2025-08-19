using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace HNTAS.Web.UI.Controllers
{
    public class DutyHolderController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        public DutyHolderController(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        #region No action Pages
        [HttpGet]
        public IActionResult YouHaveDeclined()
        {
            this.ShowBackButton("YouHaveBeenInvited", "DutyHolder");
            return View();
        }

        #endregion

        #region User input Pages

        [HttpGet]
        public IActionResult YouHaveBeenInvited()
        {
            var model = _sessionHelper.GetFromSession<YouHaveBeenInvitedModel>(HttpContext, SessionKeys.YouHaveBeenInvitedModelKey) ?? new YouHaveBeenInvitedModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult YouHaveBeenInvited(YouHaveBeenInvitedModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            switch (model.AcceptInvitation) {
                case "accept":
                    return RedirectToAction("StartPage", "DutyHolder");
                case "decline":
                    return RedirectToAction("YouHaveDeclined", "DutyHolder");
                default:
                    ModelState.AddModelError(nameof(model.AcceptInvitation), "Please select a valid option.");
                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult StartPage() {
            this.ShowBackButton("YouHaveBeenInvited", "DutyHolder");
            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            this.ShowBackButton("StartPage", "DutyHolder");
            var model = new DHDashboardModel() { OrganisationName = "ABC Org", HeatNetwork = "XyZ HN", HNStatus = "Active" };
            return View(model);
        }

        #endregion
    }
}
