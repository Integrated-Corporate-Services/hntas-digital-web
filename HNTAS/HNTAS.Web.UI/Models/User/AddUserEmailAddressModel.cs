using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class AddUserEmailAddressModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; } = null!;
    }
}