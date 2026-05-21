using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorDetailsViewModel
    {
        [Required(ErrorMessage="Enter their first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage="Enter their last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage="Enter their email address")]
        public string EmailAddress { get; set; }
    }
}
