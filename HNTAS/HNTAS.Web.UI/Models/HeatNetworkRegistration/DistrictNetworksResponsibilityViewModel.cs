using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class DistrictNetworksResponsibilityViewModel
    {
        [Required(ErrorMessage = "Enter the number of new connected district networks you are responsible for")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of new district networks must be a valid number")]
        [Display(Name = "Number of new district networks")]
        public int? ResponsibleDistrictNetworks { get; set; }

        /// <summary>
        /// Stores the maximum limit passed from NetworkConnectionsViewModel.NewOtherDistrictNetworks
        /// used for dynamic validation logic in the POST action.
        /// </summary>
        public int MaxAllowedDistrictNetworks { get; set; }
    }
}
