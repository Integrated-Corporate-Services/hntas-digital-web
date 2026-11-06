using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorDetailsViewModel
    {
        private string _firstName;
        private string _lastName;

        private string _jobTitle;

        [Required(ErrorMessage = "Enter the new contributor's first name")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Enter a first name that does not include < or >")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName
        {
            get => _firstName;
            set => _firstName = value?.Trim();
        }

        [Required(ErrorMessage = "Enter the new contributor's last name")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Enter a last name that does not include < or >")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName
        {
            get => _lastName;
            set => _lastName = value?.Trim();
        }


        [Required(ErrorMessage = "Enter the new contributor's job title")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&]+$", ErrorMessage = "Enter a valid job title using letters, numbers or common punctuation only")]
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string JobTitle
        {
            get => _jobTitle;
            set => _jobTitle = value?.Trim();
        }

        public string? EmailAddress { get; set; }

        public bool InviteForFirstTime { get; set; }

        public string? WorkingOrgName { get; set; }
    }
}
