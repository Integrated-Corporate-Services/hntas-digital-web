using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class ContributorContactDetailsModel
    {

        [Required(ErrorMessage = "Enter your first name.")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "First name can only contain letters, spaces and hyphen(-).")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Enter your last name.")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "Last name can only contain letters, spaces and hyphen(-).")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? LastName { get; set; }
    }
}
