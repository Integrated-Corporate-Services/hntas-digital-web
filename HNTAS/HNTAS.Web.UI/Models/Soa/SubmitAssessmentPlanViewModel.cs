namespace HNTAS.Web.UI.Models.Soa
{
    public class SubmitAssessmentPlanViewModel
    {
        public string DocumentName { get; set; }
        public int PhaseNumber { get; set; }

        public List<StepNavItem> Steps { get; set; } = new List<StepNavItem>();
    }
}
