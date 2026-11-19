using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class MainEnergyCentreStatusModel
    {
        [Required(ErrorMessage = "Select if the main energy centre is existing or new")]
        public string? MainEnergyCentreStatus { get; set; }
    }
}
