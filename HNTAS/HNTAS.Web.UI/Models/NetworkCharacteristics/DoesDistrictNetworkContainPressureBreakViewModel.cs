using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkCharacteristics
{
    public class DoesDistrictNetworkContainPressureBreakViewModel
    {
        [Required(ErrorMessage = "Please select an option")]
        public string ContainsPressureBreak { get; set; }
    }
}
