using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Address
{
    public class SearchAddressByPostcodeModel
    {
        [Required(ErrorMessage = "Please enter the postcode.")]
        [RegularExpression(@"^(GIR 0AA|[A-PR-UWYZ](?:[0-9]{1,2}|[A-HK-Y][0-9]{1,2}|[0-9][A-HJKS-UW]|[A-HK-Y][0-9][ABEHMNPRV-Y]) ?[0-9][ABD-HJLNP-UW-Z]{2})$",
        ErrorMessage = "Please enter a valid UK postcode.")]
        public string? Postcode { get; set; }
        public string[]? Addresses { get; set; }
        public string? SelectedFullAddress { get; set; }
    }
}
