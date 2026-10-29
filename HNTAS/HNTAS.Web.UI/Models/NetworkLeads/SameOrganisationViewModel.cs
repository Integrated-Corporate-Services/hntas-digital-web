using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class SameOrganisationViewModel
    {
        [Required(ErrorMessage = "Select yes whether you and the network manager work for the same organisation")]
        public bool? WorksForSameOrganisation { get; set; }

        // Conditional validation: Required only if WorksForSameOrganisation is false
        [Required(ErrorMessage = "Enter the organisation name")]
        [RegularExpression(@"^[A-Za-z0-9&@£$€¥#\.,:;\- ]+$", ErrorMessage = "Organisation name contains invalid characters")]
        public string? OrganisationName { get; set; }
    }
}
