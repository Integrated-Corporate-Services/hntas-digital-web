using System.ComponentModel.DataAnnotations;
using HNTAS.Web.UI.ModelValidation;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class NewLeadDetailsViewModel
    {
        [Required(ErrorMessage = "Enter the new manager's first name")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Enter a first name that does not include < or >")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName 
        { 
            get => _firstName; 
            set => _firstName = value?.Trim(); 
        }

        private string? _firstName;

        [Required(ErrorMessage = "Enter the new manager's last name")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Enter a last name that does not include < or >")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName 
        { 
            get => _lastName; 
            set => _lastName = value?.Trim(); 
        }

        private string? _lastName;

        [Required(ErrorMessage = "Enter the new manager's job title")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&]+$", ErrorMessage = "Enter a valid job title using letters, numbers or common punctuation only")]
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string? JobTitle
        {
            get => _jobTitle;
            set => _jobTitle = value?.Trim();
        }

        private string? _jobTitle;

        public bool InviteForFirstTime { get; set; }

        public string? WorkingOrgName { get; set; }

        [MustBeTrue(ErrorMessage = "Confirm you are authorised to give this person a permission to add heat networks for this organisation")]
        public bool ConfirmedDeclaration { get; set; }
    }
}
