using HNTAS.Api.Client.Model;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class SwitchOrganisationModel
    {
        //public List<Organisation> Organisations { get; set; }
        public List<OrganisationToSelect> Organisations { get; set; }
        [Required(ErrorMessage = "Please select an option")]
        public string SelectedOrganisation { get; set; }
    }

    public class OrganisationToSelect
    {
        public string OrgId { get; set; }
        public string OrgName { get; set; }
    }
}
