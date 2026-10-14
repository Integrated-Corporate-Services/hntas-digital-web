using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class NetworkSupplyTypeModel
    {
        [Required(ErrorMessage = "Select whether this heat network has a main energy centre")]
        public string? EnergyCentreOption { get; set; }
    }
}
