using System.ComponentModel.DataAnnotations;
using HNTAS.Web.UI.ModelValidation;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class NewLeadDetailsViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "First name can only contain letters and spaces.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Last name can only contain letters and spaces.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        public string EmailId { get; set; }
        [MustBeTrue(ErrorMessage = "Confirm you are authorised to give this person a permission to add heat networks for this organisation.")]
        public bool ConfirmedDeclaration { get; set; }
    }
}
