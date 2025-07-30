using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class HeatNetworkNameModel
    {
        [Required(ErrorMessage = "Please enter the heat network name.")]
        [StringLength(100, ErrorMessage = "The heat network name cannot exceed 100 characters.")]
        public string HeatNetworkName { get; set; }
    }

    public class HeatNetworkLocationModel
    {
        [Required(ErrorMessage = "Please enter the What3words url.")]
        public string HeatNetworkLocation { get; set; }
    }

    public class CheckYourAnswersHeatNetworkModel
    {
        public HeatNetworkNameModel HeatNetworkNameModel { get; set; }
        public HeatNetworkLocationModel HeatNetworkLocationModel { get; set; }

        // The ConfirmedDeclaration property, now part of this specific ViewModel
        [Display(Name = "I confirm that")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the declaration to proceed.")]
        public bool ConfirmedDeclaration { get; set; }
    }

}
