using HNTAS.Web.UI.CustomValidation;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class SubstationsViewModel
    {
        [Required(ErrorMessage = "Please select an option")]
        public bool? HasDistrictSubstation { get; set; } = null;
        //[Required(ErrorMessage = "Enter the number of substations")]
        [Range(1, 999, ErrorMessage = "Value must be between 1 and 999")]
        public int? NumberOfSubstations { get; set; }
    }
}
