using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class NewLeadAlreadyExistsViewModel
    {
        public string OrganisationName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select whether this is the person you want to invite")]
        public bool? IsCorrectPerson { get; set; }
    }
}
