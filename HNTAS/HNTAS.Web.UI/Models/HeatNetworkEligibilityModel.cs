using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class  AreYouTheRPModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? AreYouTheRP { get; set; }
    }
    public class  IsYourOrgWorkingOnANewHNModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? IsYourOrgWorkingOnANewHN { get; set; }
    }
    public class  IsHNLocatedInEnglandScotlandWalesModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? IsHNLocatedInEnglandScotlandWales { get; set; }
    }  
    public class  HowManyDwellingsIncludedModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string? HowManyDwellingsIncluded { get; set; }
    }
}