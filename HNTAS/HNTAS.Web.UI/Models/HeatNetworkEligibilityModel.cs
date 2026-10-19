using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class  AreYouTheRPModel
    {
        [Required(ErrorMessage = "Select whether you are responsible for heat networks in your organisation")]
        public string? AreYouTheRP { get; set; }
    }
    public class  IsYourOrgWorkingOnANewHNModel
    {
        [Required(ErrorMessage = "Select whether your organisation is working on any new heat networks")]
        public string? IsYourOrgWorkingOnANewHN { get; set; }
    }
    public class  IsHNLocatedInEnglandScotlandWalesModel
    {
        [Required(ErrorMessage = "Select whether any of your new heat networks are in England, Scotland or Wales")]
        public string? IsHNLocatedInEnglandScotlandWales { get; set; }
    }  
    public class  HowManyDwellingsIncludedModel
    {
        [Required(ErrorMessage = "Select whether the network has (or helps supply networks with) 6 or more dwellings or units")]
        public string HowManyDwellingsIncluded { get; set; }
    }
}