using HNTAS.Api.Client.Model;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class AssessorAssessmentSelectionViewModel
    {
        public List<AssessmentOption> AssessmentOptions { get; set; } = [];
        //[Required(ErrorMessage = "Select the assessment before continuing.")]
        public string? SelectedAssessmentOption { get; set; }
        public ElementTypeInShort ElementType { get; set; }
    }

    public class AssessmentOption
    {
        public string Label { get; set; } = null!;
        public string Hint { get; set; } = null!;
        public bool IsDisabled { get; set; }
    }
}
