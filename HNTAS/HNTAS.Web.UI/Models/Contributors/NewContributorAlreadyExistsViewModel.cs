using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorAlreadyExistsViewModel
    {
        //public string OrganisationName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select whether this is the person you want to invite")]
        public bool? IsCorrectPerson { get; set; }
    }
}
