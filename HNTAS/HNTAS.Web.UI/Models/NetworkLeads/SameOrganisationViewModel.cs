using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class SameOrganisationViewModel
    {
        public bool? WorksForSameOrganisation { get; set; }

        [RegularExpression(@"^[A-Za-z0-9&@£$€¥#\.,:;\- ]+$", ErrorMessage = "Organisation name contains invalid characters")]
        public string? OrganisationName { get; set; }
    }
}
