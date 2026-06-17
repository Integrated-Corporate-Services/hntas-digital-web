using HNTAS.Web.UI.Workflows.Enums;

namespace HNTAS.Web.UI.Workflows.Models
{
    public class WorkflowState<TData>
    {
        public WorkflowType WorkflowType { get; set; }
        public int CurrentStep { get; set; }
        public List<int> CompletedSteps { get; set; } = new List<int>();
        public TData Data { get; set; }
    }
}
