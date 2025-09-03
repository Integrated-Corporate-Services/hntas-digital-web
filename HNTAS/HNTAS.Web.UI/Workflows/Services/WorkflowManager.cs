using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models;

namespace HNTAS.Web.UI.Workflows.Services
{
    public class WorkflowManager : IWorkflowManager
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkflowManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetSessionKey<TData>()
        {
            // Use the type name of the data model to create a unique key
            return $"WorkflowState_{typeof(TData).Name}";
        }

        public WorkflowState<TData> GetState<TData>()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var key = GetSessionKey<TData>();
            var stateJson = session.GetString(key);

            if (string.IsNullOrEmpty(stateJson))
            {
                return null; // Return null if no state exists
            }

            return System.Text.Json.JsonSerializer.Deserialize<WorkflowState<TData>>(stateJson);
        }

        public void SaveState<TData>(WorkflowState<TData> state)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var key = GetSessionKey<TData>();
            var stateJson = System.Text.Json.JsonSerializer.Serialize(state);
            session.SetString(key, stateJson);
        }

        public void ClearState<TData>()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var key = GetSessionKey<TData>();
            session.Remove(key);
        }

        public void SaveAndAdvance<TData>(WorkflowState<TData> state, TData currentStepData, int nextStep)
        {
            // Update the state's data object with the latest information.
            // This assumes the TData object passed in contains the complete, updated data.
            state.Data = currentStepData;

            // Increment the current step.
            state.CurrentStep = nextStep;

            // Add the just-completed step to the list of completed steps.
            // We assume the completed step is the one just before the next step.
            if (nextStep > 1)
            {
                state.CompletedSteps.Add(nextStep - 1);
            }

            // Save the updated state to the session.
            SaveState(state);
        }


        public WorkflowState<T> StartWorkflow<T>(WorkflowType workflowType, Enum initialStep) where T : class, new()
        {
            var state = new WorkflowState<T>
            {
                WorkflowType = workflowType,
                CurrentStep = Convert.ToInt32(initialStep),
                Data = new T()
            };

            SaveState(state);
            return state;
        }
    }
}
