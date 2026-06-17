using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class OrganisationContactDetailsModel : ContactDetailsModel
    {
        [Required(ErrorMessage = "Email address is missing.")]
        [EmailAddress(ErrorMessage = "Email address is not in the correct format.")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Enter your job title.")]
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&]+$", ErrorMessage = "Enter a valid job title using letters, numbers or common punctuation only.")]
        public string? JobTitle { get; set; }
    }
}