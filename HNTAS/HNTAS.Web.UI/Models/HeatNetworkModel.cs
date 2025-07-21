using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class HeatNetworkNameModel
    {
        [Required(ErrorMessage = "Please enter the heat network name.")]
        [StringLength(100, ErrorMessage = "The heat network name cannot exceed 100 characters.")]
        public string HeatNetworkName { get; set; }
    }
    
}
