namespace HNTAS.Web.UI.Workflows
{
    public interface IWorkflowModel<TStep> where TStep : Enum
    {
        TStep CurrentStep { get; set; }
        void AdvanceToStep(TStep nextStep);
        object? GetStepData(TStep step);
    }
}
