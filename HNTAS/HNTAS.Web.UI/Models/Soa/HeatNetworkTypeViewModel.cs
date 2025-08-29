using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Soa
{
    public class HeatNetworkTypeViewModel
    {
        // This list will hold the display text and value for each radio button
        public List<SelectItemOption> HeatNetworkTypes { get; set; } = new List<SelectItemOption>();

        [Required(ErrorMessage = "Select what is your heat network type.")]
        public HeatNetworkType? SelectedHNType { get; set; }

        public string? OtherNetworkDescription { get; set; }
    }
}
