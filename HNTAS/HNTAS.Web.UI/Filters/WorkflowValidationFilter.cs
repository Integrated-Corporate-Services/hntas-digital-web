using HNTAS.Web.UI.Workflows;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HNTAS.Web.UI.Filters
{
    public class WorkflowValidationFilter<TWorkflow, TStep> : IActionFilter
    where TWorkflow : class, IWorkflowModel<TStep>
    where TStep : Enum
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly TStep _expectedStep;
        private readonly IRedirectResolver<TWorkflow, TStep> _redirectResolver;

        public WorkflowValidationFilter(
            IWorkflowManager workflowManager,
            TStep expectedStep,
            IRedirectResolver<TWorkflow, TStep> redirectResolver)
        {
            _workflowManager = workflowManager ?? throw new ArgumentNullException(nameof(workflowManager));
            _expectedStep = expectedStep;
            _redirectResolver = redirectResolver ?? throw new ArgumentNullException(nameof(redirectResolver));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var state = _workflowManager.GetState<TWorkflow>();

            if (state?.Data == null)
            {
                context.Result = _redirectResolver.Resolve(null, _expectedStep);
                return;
            }

            var currentStep = Convert.ToInt32(state.Data.CurrentStep);
            var expectedStep = Convert.ToInt32(_expectedStep);

            // Block only if trying to jump forward beyond current step
            if (expectedStep > currentStep)
            {
                context.Result = _redirectResolver.Resolve(state.Data, _expectedStep);
            }
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No post-processing needed for now
        }
    }



}
