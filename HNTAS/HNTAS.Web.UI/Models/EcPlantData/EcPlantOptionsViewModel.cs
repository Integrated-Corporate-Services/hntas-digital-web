using HNTAS.Web.UI.CustomValidation;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.EcPlantData
{
    public class EcPlantOptionsViewModel
    {
        [Required(ErrorMessage = "Enter number of combined heat and power systems")]
        [Range(0, 9999, ErrorMessage = "Enter a whole number of 0 or more for number of combined heat and power systems")]
        public int? NumberOfChp { get; set; }
        [Required(ErrorMessage = "Enter number of heat pump systems")]
        [Range(0, 9999, ErrorMessage = "Enter a whole number of 0 or more for number of heat pump systems")]
        public int? NumberOfHeatPump { get; set; }
        [Required(ErrorMessage = "Enter number of heat recovered without heat pumps")]
        [Range(0, 9999, ErrorMessage = "Enter a whole number of 0 or more for number of heat recovered without heat pumps")]
        public int? NumberOfHighHeatRecoverySources { get; set; }
        [Required(ErrorMessage = "Enter number of boiler plants")]
        [Range(0, 9999, ErrorMessage = "Enter a whole number of 0 or more for number of boiler plants")]
        public int? NumberOfBoilerPlants { get; set; }
    }
}
