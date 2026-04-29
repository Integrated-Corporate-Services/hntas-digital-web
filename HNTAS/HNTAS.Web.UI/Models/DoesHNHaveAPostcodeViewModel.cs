using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class DoesHNHaveAPostcodeViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Select yes if it has a postcode.")]
        public bool? HasPostcode { get; set; } = null;

        [RegularExpression(@"^[A-Za-z]{1,2}\d[A-Za-z\d]?\s*\d[A-Za-z]{2}$",
            ErrorMessage = "Please enter a valid UK postcode.")]
        public string? Postcode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((bool)HasPostcode!)
            {
                if (string.IsNullOrWhiteSpace(Postcode))
                {
                    yield return new ValidationResult(
                        "Enter a postcode.",
                        new[] { nameof(Postcode) }
                    );
                }
            }
        }
    }
}