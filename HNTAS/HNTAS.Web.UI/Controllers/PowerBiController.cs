using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class PowerBiController : Controller
    {
        private readonly ISessionHelper _sessionHelper;

        public PowerBiController(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        public IActionResult Index()
        {
            var isSuperUser = _sessionHelper.GetFromSession<bool?>(HttpContext, SessionKeys.IsSuperUserKey);

            ViewBag.OrgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);
            ViewBag.IsSuperUser = isSuperUser;
            return View();
        }
    }
}
