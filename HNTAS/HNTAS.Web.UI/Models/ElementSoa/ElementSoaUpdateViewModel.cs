using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.CustomValidation;
using HNTAS.Web.UI.Models.NetworkElements;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    //public class ElementSoaUpdateStatusViewModel
    //{
    //    //public List<string> SoaStatus { get; set; } = [];
    //    //[Required(ErrorMessage = "Select the current stage")]
    //    //public string? SelectedSoaStatus { get; set; }
    //    public List<SoaStatusWithCount> SoaStatus { get; set; } = [];
    //    public SoaStage? SoaStage { get; set; }
    //    public string? ElementId { get; set; }
    //    public HeatNetworkElementType? Type { get; set; }
    //    public string? ElementName { get; set; }
    //    public string? SoaPhase { get; set; }
    //}

    public class ElementSoaUpdateStatusViewModel
    {
        public SoaStage? SoaStage { get; set; }
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

    public class SoaStatusOption
    {
        public SoaStatus Id { get; set; }        
    }


}
