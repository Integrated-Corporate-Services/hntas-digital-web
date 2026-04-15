using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Workflows.Validation
{
    public class ExistingContributorRedirectResolver : IRedirectResolver<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>
    {
        public IActionResult Resolve(AddExistingContributorWorkflowModel workflow, ExistingContributorWorkflowStep expectedStep)
        {
            return new RedirectToActionResult("Index", "Home", null);
        }
    }
}
