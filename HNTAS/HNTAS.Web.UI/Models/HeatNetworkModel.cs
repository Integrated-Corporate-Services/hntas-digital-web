using HNTAS.Web.UI.Models;
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

    public class HeatNetworkPhaseModel
    {
        [Required(ErrorMessage = "Please select the heat network phase.")]
        public string HeatNetworkPhase { get; set; }
    }

    public class HasElementBeenRegisteredModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? HasElementBeenRegistered { get; set; }
    }

    public class HasPlanningApplicationBeenSubmittedModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? HasPlanningApplicationBeenSubmitted { get; set; }
    }

    public class HaveYouSignedMEContractModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? HaveYouSignedMEContract { get; set; }
    }

    public class PathwayModel
    {
        public string Pathway { get; set; }
    }

    public class CheckYourAnswersHeatNetworkModel
    {
        public HeatNetworkNameModel HeatNetworkNameModel { get; set; }
        public HeatNetworkLocationModel HeatNetworkLocationModel { get; set; }
        public HeatNetworkPhaseModel HeatNetworkPhaseModel { get; set; }
        public HasElementBeenRegisteredModel? HasElementBeenRegisteredModel { get; set; }
        public HasPlanningApplicationBeenSubmittedModel? HasPlanningApplicationBeenSubmittedModel { get; set; }
        public HaveYouSignedMEContractModel HaveYouSignedMEContractModel { get; set; }
        public PathwayModel PathwayModel { get; set; }

        // The ConfirmedDeclaration property, now part of this specific ViewModel
        [Display(Name = "I confirm that")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the declaration to proceed.")]
        public bool ConfirmedDeclaration { get; set; }
    }

}
