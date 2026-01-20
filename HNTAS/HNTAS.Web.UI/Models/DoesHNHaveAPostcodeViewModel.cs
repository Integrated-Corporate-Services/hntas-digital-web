using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class DoesHNHaveAPostcodeViewModel
    {
        [Required(ErrorMessage = "Select yes if it has a postcode.")]
        public bool HasPostcode { get; set; }

        [RegularExpression(@"^[A-Za-z]{1,2}\d[A-Za-z\d]?\s*\d[A-Za-z]{2}$",
            ErrorMessage = "Please enter a valid UK postcode.")]
        public string? Postcode { get; set; }

    }
}