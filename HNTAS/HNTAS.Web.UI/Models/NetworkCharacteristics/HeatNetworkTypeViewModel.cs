using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkCharacteristics
{
    public class HeatNetworkTypeOption
    {
        public string Id { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string Hint { get; set; } = null!;
        public string SummaryText { get; set; } = null!;
        public string DetailsText { get; set; } = null!;
    }
    public class HeatNetworkTypeViewModel
    {
        public List<HeatNetworkTypeOption>? HeatNetworkTypes {get; set; }
        [Required(ErrorMessage = "Select the heat network type")]
        public string SelectedHeatNetworkType { get; set; }
        public HNTAS.Api.Client.Model.HeatNetworkType? SelectedHeatNetworkTypeInEnum { get; set; }
        public string? SelectedHeatNetworkTypeToDisplay { get; set; }
    }
}
