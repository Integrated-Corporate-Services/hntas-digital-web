using HNTAS.Web.UI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.OrganisationRole
{
    public class RoleAssignmentModel
    {
        public List<SelectItemOption> AvailableRoles { get; set; } = new List<SelectItemOption>();

        [Required(ErrorMessage = "Select a role")]
        public string SelectedRoleName { get; set; } = null!;
        public string? InvitedUserName { get; set; }
    }
}
