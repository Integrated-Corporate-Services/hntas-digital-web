using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.CompaniesHouse;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HNTAS.Web.UI.Models.Address
{
    public class AddressByStreetOrTownModel
    {
        // Initialize all values with a default value to avoid CS8618
        [Required(ErrorMessage = "Enter the street address")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Street address must not include < or >")]
        public string? StreetAddress { get; set; }

        [Required(ErrorMessage = "Enter the town or city")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Town or city must not include < or >")]
        public string? TownOrCity { get; set; }

        //[Required(ErrorMessage = "Enter the postcode")]
        //[RegularExpression(@"^[^<>]*$", ErrorMessage = "Postal Code must not include < or >")]
        public string? Postalcode { get; set; }

        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Country must not include < or >")]
        public string? Country { get; set; }

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
