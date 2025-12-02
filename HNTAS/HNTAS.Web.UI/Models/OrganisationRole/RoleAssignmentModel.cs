using HNTAS.Web.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.OrganisationRole
{
    public class RoleAssignmentModel
    {
        public string? UserName { get; set; }
        public string? ExistingRPName { get; set; }

        // Data captured from the radio buttons
        [Required(ErrorMessage = "Select the role you want to assign to this user.")]
        public RoleAssignmentType SelectedRoleType { get; set; }
    }
}
