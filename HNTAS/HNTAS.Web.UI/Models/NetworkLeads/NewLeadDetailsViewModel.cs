using System.ComponentModel.DataAnnotations;
using HNTAS.Web.UI.ModelValidation;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class NewLeadDetailsViewModel
    {
        [Required(ErrorMessage = "Enter the new manager's first name")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "First name can only contain letters, spaces and hyphen(-)")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName 
        { 
            get => _firstName; 
            set => _firstName = value?.Trim(); 
        }

        private string? _firstName;

        [Required(ErrorMessage = "Enter the new manager's last name")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "Last name can only contain letters, spaces and hyphen(-)")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName 
        { 
            get => _lastName; 
            set => _lastName = value?.Trim(); 
        }

        private string? _lastName;

        [Required(ErrorMessage = "Enter the new manager's job title")]
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
