using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Workflows.Validation
{
    public class NewContributorRedirectResolver : IRedirectResolver<AddNewContributorWorkflowModel, ContributorWorkflowStep>
    {
        public IActionResult Resolve(AddNewContributorWorkflowModel workflow, ContributorWorkflowStep expectedStep)
        {
            return new RedirectToActionResult("Index", "Home", null);
        }
    }

}
