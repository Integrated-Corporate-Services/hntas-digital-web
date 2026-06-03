using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkCharacteristics
{
    public class CommunalFloorsViewModel
    {
        [Required(ErrorMessage = "Please enter a value.")]
        public int NumberOfCommunalFloors { get; set; }
    }
}
