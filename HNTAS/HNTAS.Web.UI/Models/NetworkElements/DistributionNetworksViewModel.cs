using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class DistributionNetworksViewModel
    {
        [Required(ErrorMessage = "Enter the number of district distribution networks")]
        [Range(1, 999, ErrorMessage = "Value must be between 1 and 999")]
        public int? NumberOfDistributionNetworks { get; set; }
    }
}
