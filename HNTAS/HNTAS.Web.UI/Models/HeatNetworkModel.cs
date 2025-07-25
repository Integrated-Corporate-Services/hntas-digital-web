using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class HeatNetworkNameModel
    {
        [Required(ErrorMessage = "Please enter the heat network name.")]
        [StringLength(100, ErrorMessage = "The heat network name cannot exceed 100 characters.")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "The heat network name can only contain numbers or letters.")]
        public string HeatNetworkName { get; set; }
    }

    public class HeatNetworkLocationModel
    {
        [Required(ErrorMessage = "Please enter the What3words url.")]
        public string HeatNetworkLocation { get; set; }
    }

}
