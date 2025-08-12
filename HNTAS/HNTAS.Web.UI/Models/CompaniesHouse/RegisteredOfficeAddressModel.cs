using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.CompaniesHouse
{
    public class RegisteredOfficeAddressModel
    {
        [Required(ErrorMessage = "Street address is required.")]
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Town or city is required.")]
        public string? Locality { get; set; } // City/Town

        [Required(ErrorMessage = "Postal code or zip code is required.")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string? Country { get; set; }
    }
}
