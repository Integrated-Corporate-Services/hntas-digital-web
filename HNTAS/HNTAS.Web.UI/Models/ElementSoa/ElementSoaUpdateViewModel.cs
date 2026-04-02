using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.NetworkElements;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ElementSoaUpdateStatusViewModel
    {
        public List<string> SoaStatus { get; set; } = [];
        [Required(ErrorMessage = "Select the current stage")]
        public string? SelectedSoaStatus { get; set; }
        public SoaStage? SoaStage { get; set; }
        public string? ElementId { get; set; }
        public HeatNetworkElementDisplayType? Type { get; set; }
        public string? ElementName { get; set; }
        public string? SoaPhase { get; set; }
    }

    public class AssessorAssessmentSelectionViewModel
    {
        public List<AssessmentOption> AssessmentOptions { get; set; } = [];
        [Required(ErrorMessage = "Select the assessment before continuing.")]
        public string? SelectedAssessmentOption { get; set; }
    }

    public class AssessmentOption
    {
        public string Label { get; set; } = null!;
        public string Hint { get; set; } = null!;
    }

    public class AssessorDetails
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class AssessorElementSelectionOverviewModel
    {
        public AssessorDetails AssessorDetails { get; set; } = null!;
        public AssessorSelectElementsViewModel AssessorSelectedElements { get; set; } = null!;
        public AssessorAssessmentSelectionViewModel AssessorAssessment { get; set; } = null!;
        public string HeatNetworkPhase { get; set; } = null!;
        public string HeatNetworkStage { get; set; } = null!;
    }


}
