
namespace HNTAS.Web.UI.Models.CompaniesHouse
{
    public class RegisteredOfficeAddressModel
    {
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        public string? Locality { get; set; } // City/Town

        public string? PostalCode { get; set; }

        public string? Country { get; set; }

        public static implicit operator RegisteredOfficeAddressModel(ManualOfficeAddressModel v)
        {
            if (v == null) return null!;
            return new RegisteredOfficeAddressModel
            {
                AddressLine1 = v.AddressLine1,
                AddressLine2 = v.AddressLine2,
                Locality = v.Locality,
                PostalCode = v.PostalCode,
                Country = v.Country
            };
        }
    }
}
