using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class OtherOrganisationNameModel
    {
        [Required(ErrorMessage = "Enter the name of your organisation")]
        [RegularExpression(@"^[A-Za-z0-9&@£$€¥#\.,:;\- ]+$", ErrorMessage = "Organisation name contains invalid characters")]
        public string? OrganisationName { get; set; }
    }
}