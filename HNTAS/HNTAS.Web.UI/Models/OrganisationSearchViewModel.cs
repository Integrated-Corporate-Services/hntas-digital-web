using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class OrganisationSearchViewModel
    {
        [Required(ErrorMessage = "Enter an Organisation ID or Name")]
        [Display(Name = "Organisation ID or Name")]
        public string SearchTerm { get; set; } = string.Empty;
    }
}
