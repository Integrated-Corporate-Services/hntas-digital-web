using HNTAS.Web.UI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class SelectRoleModel
    {
        [Required(ErrorMessage = "Choose the role.")]
        public string SelectedRoleId { get; set; } = null!;

        public string? SelectedRoleName { get; set; }

        public List<SelectItemOption> Roles { get; set; } = [];
    }
}
