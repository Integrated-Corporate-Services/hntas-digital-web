using System.ComponentModel.DataAnnotations;
using HNTAS.Web.UI.Models.Enums;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class HeatNetworkTypeViewModel
    {
        [Required(ErrorMessage = "Please select a heat network type")]
        public HeatNetworkType HeatNetworkType { get; set; }
    }
}
