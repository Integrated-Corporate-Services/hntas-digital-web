using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class AddUserEmailAddressModel
    {

        [RegularExpression(
          @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
          ErrorMessage = "Invalid email format."
        )]
        [Required(ErrorMessage = "Email address is required.")]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; } = null!;
    }
}