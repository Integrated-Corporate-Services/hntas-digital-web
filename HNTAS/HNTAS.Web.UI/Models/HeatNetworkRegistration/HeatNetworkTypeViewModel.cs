using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class HeatNetworkTypeViewModel
    {
        [Required(ErrorMessage = "Please select a heat network type")]
        public string HeatNetworkType { get; set; }
    }
}
