using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorDetailsViewModel
    {
        private string _firstName;
        private string _lastName;

        private string _jobTitle;

        [Required(ErrorMessage = "Enter the new contributor's first name")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "Enter a valid first name using letters, numbers or common punctuation only")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName
        {
            get => _firstName;
            set => _firstName = value?.Trim();
        }

        [Required(ErrorMessage = "Enter the new contributor's last name")]
        [RegularExpression(@"^[a-zA-Z\-]+$", ErrorMessage = "Enter a valid last name using letters, numbers or common punctuation only")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName
        {
            get => _lastName;
            set => _lastName = value?.Trim();
        }


        [Required(ErrorMessage = "Enter the new contributor's job title")]
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
