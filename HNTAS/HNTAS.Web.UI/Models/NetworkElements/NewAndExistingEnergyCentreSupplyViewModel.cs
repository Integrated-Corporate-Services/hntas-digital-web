using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class NewAndExistingEnergyCentreSupplyViewModel
    {
        [Required(ErrorMessage = "Select whether your existing energy centre supplies any of your new buildings")]
        public string IsExistingEcSupply { get; set; }
    }
}
