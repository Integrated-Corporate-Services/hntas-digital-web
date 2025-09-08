using HNTAS.Web.UI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetwork
{
    public class ChooseHeatNetworkModel
    {
        [Required(ErrorMessage = "Choose the heat network.")]
        public string SelectedHeatNetworkId { get; set; } = null!;

        public string? SelectedHeatNetworkName { get; set; }

        public List<SelectItemOption> HeatNetworks { get; set; } = [];
    }
}
