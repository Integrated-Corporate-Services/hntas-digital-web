using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class OtherOrganisationNameModel
    {
        [Required(ErrorMessage = "Enter the name of your organisation")]
        public string? OrganisationName { get; set; }
    }
}