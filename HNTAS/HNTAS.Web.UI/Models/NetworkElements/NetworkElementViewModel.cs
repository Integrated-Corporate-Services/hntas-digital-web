using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.CustomValidation;
using HNTAS.Web.UI.Models.Soa;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class NetworkElementViewModel
    {
        public List<NetworkElementOption> ElementOptions { get; set; } = new();

        [MustHaveOneItem(ErrorMessage = "Select at least one element that is part of your heat network.")]
        public List<HeatNetworkElementDisplayType> SelectedElementIds { get; set; } = new();
        public Dictionary<HeatNetworkElementDisplayType, int?> ElementCounts { get; set; } = new();
    }

    public class NetworkElementOption
    {
        public HeatNetworkElementDisplayType Id { get; set; }
        public string Label { get; set; } = null!;
        public string SubLabel { get; set; } = null!;
        public string Hint { get; set; } = null!;
    }
}
