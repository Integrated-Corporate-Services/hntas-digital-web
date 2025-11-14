using HNTAS.Web.UI.Models.CompaniesHouse;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Address
{
    public class AddressByStreetOrTownModel
    {
        // Initialize all values with a default value to avoid CS8618

        [Required(ErrorMessage = "Street address is required.")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Town or city is required.")]
        public string TownOrCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal Code or Zip code is required.")]
        public string Postalcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        public string Country { get; set; } = string.Empty;
        public string Fulladdress { get; set; } = string.Empty;

        public static implicit operator AddressByStreetOrTownModel(RegisteredOfficeAddressModel v)
        {
            if (v == null) return null!;
            return new AddressByStreetOrTownModel
            {
                 StreetAddress = v.AddressLine1,
                 TownOrCity = v.Locality,
                 Postalcode = v.PostalCode,
                 Country = v.Country                
            };
        }
    }
}
