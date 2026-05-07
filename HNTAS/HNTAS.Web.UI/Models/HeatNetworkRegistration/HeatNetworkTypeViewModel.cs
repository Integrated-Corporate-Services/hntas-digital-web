using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class IsHnTypeCommunalViewModel
    {
        [Required(ErrorMessage = "Please select a heat network type")]
        public bool IsHnTypeCommunal { get; set; }
    }

    public class DoesCommunalHnHaveOwnEcViewModel
    {
        [Required(ErrorMessage = "Please select whether the communal heat network has its own energy centre")]
        public bool HasOwnEc { get; set; }
    }

    public class DoesDistrictHnHaveOwnEcViewModel
    {
        [Required(ErrorMessage = "Please select whether the district heat network has its own energy centre")]
        public bool HasOwnEc { get; set; }
    }

    public class DoesCommunalEcSupplyOneBlockViewModel
    {
        [Required(ErrorMessage = "Please select whether the communal energy centre supplies only one block or more than one block")]
        public bool SuppliesOneBlock { get; set; }
    }

}