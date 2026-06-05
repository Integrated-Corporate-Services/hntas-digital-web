using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Address
{
    public class SearchAddressByPostcodeModel
    {
        [Required(ErrorMessage = "Please enter the postcode.")]
        [RegularExpression(@"^[A-Za-z]{1,2}\d[A-Za-z\d]?\s*\d[A-Za-z]{2}$",
        ErrorMessage = "Please enter a valid UK postcode.")]
        public string? Postcode { get; set; }
        public string[]? Addresses { get; set; }
        public string? SelectedFullAddress { get; set; }
    }
}
