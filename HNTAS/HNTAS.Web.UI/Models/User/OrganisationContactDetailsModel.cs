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
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Job title can only contain letters and spaces.")]
        public string? JobTitle { get; set; }
    }
}