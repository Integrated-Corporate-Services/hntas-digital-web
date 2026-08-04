using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.CompaniesHouse;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Address
{
    public class AddressByStreetOrTownModel
    {
        // Initialize all values with a default value to avoid CS8618

        [Required(ErrorMessage = "Street address is required.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Street address must not include < or >")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Town or city is required.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Town or city must not include < or >")]
        public string TownOrCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal Code or Zip code is required.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Postal Code or Zip code must not include < or >")]
        public string Postalcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Country must not include < or >")]
        public string Country { get; set; } = string.Empty;
        public string Fulladdress { get; set; } = string.Empty;

        public static implicit operator AddressByStreetOrTownModel(RegisteredOfficeAddressModel v)
        {
            if (v == null) return null!;
            return new AddressByStreetOrTownModel
            {
                 StreetAddress = v.AddressLine1 ?? string.Empty,
                 TownOrCity = v.Locality ?? string.Empty,
                 Postalcode = (v.PostalCode ?? string.Empty).ToUpper(),
                 Country = v.Country ?? string.Empty
            };
        }
        public static implicit operator AddressByStreetOrTownModel(RegisteredAddress v)
        {
            if (v == null) return null!;
            return new AddressByStreetOrTownModel
            {
                StreetAddress = v.AddressLine1,
                TownOrCity = v.Town!,
                Postalcode = v.Postcode!,
                Country = v.Country!,
                Fulladdress = $"{v.AddressLine1}, {v.Town}, {v.Postcode.ToUpper()}, {v.Country}"
            };
        }
    }
}
