using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.CustomValidation;

namespace HNTAS.Web.UI.Models.ElementSoa
{   

    public class ExistingElementSoaUpdateStatusViewModel
    {
        public Milestone? Milestone { get; set; }
        public string? ElementId { get; set; }
        public HeatNetworkElementType? ElementDisplayType { get; set; }
        public string? ElementName { get; set; }
        public int? ElementCount { get; set; }
        public ElementTypeInShort ElementType { get; set; }
        public string? SoaPhase { get; set; }

        public List<SoaStatusOption> SoaStatusOptions { get; set; } = new();

        [MustHaveOneItem(ErrorMessage = "Select at least one status.")]
        public List<SoaStatus> SelectedSoaStatusOptions { get; set; } = new();
        public Dictionary<SoaStatus, int?> SoaStatusCounts { get; set; } = new();
    }
}
