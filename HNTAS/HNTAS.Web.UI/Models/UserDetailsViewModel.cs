using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class UserDetailsViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Contact number")]
        [Phone]
        public string ContactNumber { get; set; }

        [Display(Name = "Job title")]
        public string JobTitle { get; set; }
    }
}
