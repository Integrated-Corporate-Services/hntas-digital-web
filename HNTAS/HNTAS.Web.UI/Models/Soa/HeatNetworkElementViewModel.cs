using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.CustomValidation;

namespace HNTAS.Web.UI.Models.Soa
{
    public class HeatNetworkElementViewModel
    {
        public List<HeatNetworkElementOption> ElementOptions { get; set; } = new();

        [MustHaveOneItem(ErrorMessage = "Select at least one element that is part of your heat network.")]
        public List<HeatNetworkElementDisplayType> SelectedElementIds { get; set; } = new();
        public Dictionary<HeatNetworkElementDisplayType, int?> ElementCounts { get; set; } = new();
    }

    public class HeatNetworkElementOption
    {
        public HeatNetworkElementDisplayType Id { get; set; }
        public string Label { get; set; } = null!;
        public string Hint { get; set; } = null!;
    }
}
