using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class NetworkSupplyTypeModel
    {
        [Required(ErrorMessage = "Select whether this heat network has a main energy centre")]
        public string? EnergyCentreOption { get; set; }        
        [StringLength(100, ErrorMessage = "The main energy centre name cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Za-z0-9 :;\-]+$", ErrorMessage = "The main energy centre name contains invalid characters")]
        public string? MainEnergyCentreName { get; set; }
    }
}
