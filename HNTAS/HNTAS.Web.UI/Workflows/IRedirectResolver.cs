using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Workflows
{
    public interface IRedirectResolver<TWorkflow, TStep>
      where TWorkflow : class, IWorkflowModel<TStep>
      where TStep : Enum
    {
        IActionResult Resolve(TWorkflow workflow, TStep expectedStep);
    }

}
