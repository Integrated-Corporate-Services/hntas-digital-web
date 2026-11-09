using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class InviteLeadViewModel
    {
        [Required(ErrorMessage = "Enter an email address")]
        [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        [Display(Name = "Email address")]
        public string EmailAddress 
        { 
            get => _emailAddress; 
            set => _emailAddress = value?.Trim(); 
        }

        private string? _emailAddress;
    }
}
