using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    
    public class  WhereIsTheHeatNetworkViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? PartOfTheUK { get; set; }
    }

    public class  HowManyDwellingsIncludedViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? NumberOfDwellings { get; set; }
    }

    public class  IsHNCurrentlyOperatingViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? IsCurrentlyOperating { get; set; }
    }

    public class DoesElementExistViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? DoesElementExist { get; set; }
    }

    public class  HasElementBeenRegisteredViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? HasElementBeenRegistered { get; set; }
    }

    public class HasPlanningApplicationBeenSubmittedViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? HasPlanningApplicationBeenSubmitted { get; set; }
    }

    public class  HaveYouSignedMEContractViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? HaveYouSignedMEContract { get; set; }
    }
}