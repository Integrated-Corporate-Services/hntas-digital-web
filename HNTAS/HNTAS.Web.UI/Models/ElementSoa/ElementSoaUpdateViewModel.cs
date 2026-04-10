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
}
