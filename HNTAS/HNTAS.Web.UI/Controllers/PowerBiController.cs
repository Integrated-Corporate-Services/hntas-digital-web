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
            ViewBag.OrgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);
            return View();
        }
    }
}
