using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorDetailsViewModel
    {
        [Required(ErrorMessage="Enter their first name")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "Enter a valid first name using letters, numbers or common punctuation only.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage="Enter their last name")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "Enter a valid last name using letters, numbers or common punctuation only.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage="Enter their email address")]
        public string EmailAddress { get; set; }
    }
}
