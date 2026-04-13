using HNTAS.Web.UI.Models.NetworkElements;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class AssessorElementSelectionOverviewModel
    {
        public AssessorDetails AssessorDetails { get; set; } = null!;
        public AssessorSelectElementsViewModel AssessorSelectedElements { get; set; } = null!;
        public AssessorAssessmentSelectionViewModel AssessorAssessment { get; set; } = null!;
        public string HeatNetworkPhase { get; set; } = null!;
        public string HeatNetworkStage { get; set; } = null!;
    }

    public class AssessorDetails
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullNameWithEmail { get; set; } = null!;
    }
}
