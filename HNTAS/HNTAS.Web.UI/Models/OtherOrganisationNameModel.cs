using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class OtherOrganisationNameModel
    {
        [Required(ErrorMessage = "Enter the name of your organisation")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Organisation name must not include < or >")]
        public string? OrganisationName { get; set; }
    }
}