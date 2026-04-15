using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Workflows.Validation
{
    public class ExistingOrganisationUserRedirectResolver : IRedirectResolver<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>
    {
        public IActionResult Resolve(AddExistingOrganisationUserWorkflowModel workflow, ExistingOrganisationUserWorkflowStep expectedStep)
        {
            return new RedirectToActionResult("Index", "Home", null);
        }
    }

}
