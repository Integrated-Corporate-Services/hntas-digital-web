using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class IsHnTypeCommunalViewModel
    {
        [Required(ErrorMessage = "Select the type of heat network you want to register")]
        public bool? IsHnTypeCommunal { get; set; }
    }

    public class DoesCommunalHnHaveOwnEcViewModel
    {
        [Required(ErrorMessage = "Select whether this heat network has its own energy centre")]
        public bool? HasOwnEc { get; set; }
    }

    public class DoesDistrictHnHaveOwnEcViewModel
    {
        [Required(ErrorMessage = "Select whether this heat network has its own main energy centre")]
        public bool? HasOwnEc { get; set; }
    }

    public class DoesCommunalEcSupplyOneBlockViewModel
    {
        [Required(ErrorMessage = "Select whether the energy centre only supplies this communal building")]
        public bool? SuppliesOneBlock { get; set; }
    }

}