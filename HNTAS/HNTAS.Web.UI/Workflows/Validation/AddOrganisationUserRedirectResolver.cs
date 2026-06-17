using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Workflows.Validation
{
    public class AddOrganisationUserRedirectResolver : IRedirectResolver<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>
    {
        public IActionResult Resolve(AddOrganisationUserWorkflowModel workflow, AddOrganisationUserWorkflowStep expectedStep)
        {
            return new RedirectToActionResult("Index", "Home", null);
        }
    }
}
