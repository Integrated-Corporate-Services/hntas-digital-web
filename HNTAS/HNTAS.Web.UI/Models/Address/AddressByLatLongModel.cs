using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Address
{
    public class AddressByLatLongModel
    {
        [Required(ErrorMessage = "Latitude is required.")]
        public decimal Latitude { get; set; }
        [Required(ErrorMessage = "Longitude is required.")]
        public decimal Longitude { get; set; }
    }
}
