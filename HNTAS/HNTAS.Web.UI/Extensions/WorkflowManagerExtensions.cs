using HNTAS.Web.UI.Workflows;

namespace HNTAS.Web.UI.Extensions
{
    public static class WorkflowManagerExtensions
    {
        public static void UpdateStep<TModel, TStep>(
            this IWorkflowManager manager,
            Action<TModel> updateData,
            TStep nextStep
        )
            where TModel : class, IWorkflowModel<TStep>
            where TStep : Enum
        {
            var state = manager.GetState<TModel>();
            if (state == null)
                throw new InvalidOperationException($"Workflow state for {typeof(TModel).Name} not found.");

            // Update model data
            updateData(state.Data);

            // Advance step via model logic
            state.Data.AdvanceToStep(nextStep);

            // Sync state-level step if needed
            state.CurrentStep = Convert.ToInt32(nextStep);

            manager.SaveState(state);
        }


        public static void UpdateStep<TModel, TStep>(
            this IWorkflowManager manager,
            TStep nextStep
        )
            where TModel : class, IWorkflowModel<TStep>
            where TStep : Enum
        {
            var state = manager.GetState<TModel>();
            if (state == null)
                throw new InvalidOperationException($"Workflow state for {typeof(TModel).Name} not found.");

            // Advance step via model logic
            state.Data.AdvanceToStep(nextStep);

            // Sync state-level step if needed
            state.CurrentStep = Convert.ToInt32(nextStep);

            manager.SaveState(state);
        }
    }
}
