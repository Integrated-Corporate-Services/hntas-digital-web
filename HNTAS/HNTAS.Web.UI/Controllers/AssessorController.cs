using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class AssessorController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        public AssessorController(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        public IActionResult DeclarationOfImpartiality()
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            var model = _sessionHelper.GetFromSession<DeclationOfImpartialityModel>(HttpContext, SessionKeys.DeclarationOfImpartialityModelKey) ?? new DeclationOfImpartialityModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeclarationOfImpartiality(DeclationOfImpartialityModel model) {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            if (!ModelState.IsValid || model.HasDeclaredImpartiality == false)
            {
                return View(model);
            }
            return RedirectToAction("HeatNetworkDetails", "Assessor");
        }

        [HttpGet]
        public IActionResult HeatNetworkDetails()
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            //var hnDetails = 
            return View();
        }
    }
}
