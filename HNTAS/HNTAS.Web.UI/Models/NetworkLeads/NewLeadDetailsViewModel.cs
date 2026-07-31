using System.ComponentModel.DataAnnotations;
using HNTAS.Web.UI.ModelValidation;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class NewLeadDetailsViewModel
    {
        [Required(ErrorMessage = "Enter your first name.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid first name using letters, numbers or common punctuation only.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Enter your last name.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid last name using letters, numbers or common punctuation only.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Enter an email address")]
        [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        public string EmailId { get; set; }

        [MustBeTrue(ErrorMessage = "Confirm you are authorised to give this person a permission to add heat networks for this organisation.")]
        public bool ConfirmedDeclaration { get; set; }
    }
}
