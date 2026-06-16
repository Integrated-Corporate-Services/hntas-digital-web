using HNTAS.Web.UI.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Filters
{
    public class ValidateWorkflowStepAttribute<TWorkflow, TStep> : TypeFilterAttribute
    where TWorkflow : class, IWorkflowModel<TStep>
    where TStep : Enum
    {
        public ValidateWorkflowStepAttribute(TStep expectedStep)
            : base(typeof(WorkflowValidationFilter<TWorkflow, TStep>))
        {
            Arguments = new object[] { expectedStep };
        }
    }
}
