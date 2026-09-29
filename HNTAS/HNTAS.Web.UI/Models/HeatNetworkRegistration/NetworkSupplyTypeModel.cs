using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class NetworkSupplyTypeModel
    {
        [Required(ErrorMessage = "Select whether your new heat network has or helps supply 6 or more units")]
        public string? EnergyCentreOption { get; set; }
    }
}
