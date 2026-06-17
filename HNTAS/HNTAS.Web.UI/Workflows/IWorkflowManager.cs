using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models;

namespace HNTAS.Web.UI.Workflows
{
    public interface IWorkflowManager
    {
        /// <summary>
        /// Retrieves the workflow state for the specified data type.
        /// </summary>
        /// <typeparam name="TData">The type of the workflow data model.</typeparam>
        /// <returns>The current workflow state or null if not found.</returns>
        WorkflowState<TData> GetState<TData>();

        /// <summary>
        /// Saves the workflow state for the specified data type.
        /// </summary>
        /// <typeparam name="TData">The type of the workflow data model.</typeparam>
        /// <param name="state">The workflow state to save.</param>
        void SaveState<TData>(WorkflowState<TData> state);

        /// <summary>
        /// Clears the workflow state for the specified data type.
        /// </summary>
        /// <typeparam name="TData">The type of the workflow data model.</typeparam>
        void ClearState<TData>();

        /// <summary>
        /// Saves the workflow state and advances to the next step.
        /// </summary>
        /// <typeparam name="TData">The type of the workflow data model.</typeparam>
        /// <param name="state">The current workflow state.</param>
        /// <param name="currentStepData">The updated data for the current step.</param>
        /// <param name="nextStep">The step to advance to.</param>
        void SaveAndAdvance<TData>(WorkflowState<TData> state, TData currentStepData, int nextStep);

        /// <summary>
        /// Starts a new workflow with the specified type and initial step.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="workflowType"></param>
        /// <param name="initialStep"></param>
        /// <returns></returns>
        WorkflowState<T> StartWorkflow<T>(WorkflowType workflowType, Enum initialStep) where T : class, new();
    }

}
