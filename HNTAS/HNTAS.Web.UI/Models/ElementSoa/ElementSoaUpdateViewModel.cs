using HNTAS.Api.Client.Model;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ElementSoaUpdateStatusViewModel
    {
        //public List<string> SoaStatus { get; set; } = [];
        //[Required(ErrorMessage = "Select the current stage")]
        //public string? SelectedSoaStatus { get; set; }
        public List<SoaStatusWithCount> SoaStatus { get; set; } = [];
        public SoaStage? SoaStage { get; set; }
        public string? ElementId { get; set; }
        public HeatNetworkElementType? Type { get; set; }
        public string? ElementName { get; set; }
        public string? SoaPhase { get; set; }
    }
}
