using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class CommunalBuildingResponsibilityViewModel
    {
        [Required(ErrorMessage = "Enter number of new communal building networks you are responsible for")]
        [Range(0, int.MaxValue, ErrorMessage = "Enter a whole number of 0 or more for number of new communal building networks you are responsible for")]
        public int? ResponsibleCommunalNetworks { get; set; } = 1; // Default initial value

        public int MaxAllowedCommunalBuildings { get; set; }
    }
}
