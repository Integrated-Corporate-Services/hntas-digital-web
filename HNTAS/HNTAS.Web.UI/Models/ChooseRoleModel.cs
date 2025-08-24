using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class ChooseRoleModel
    {
        [Required(ErrorMessage = "Choose the role.")]
        public string SelectedRoleId { get; set; } = null!;

        public string? SelectedRoleName { get; set; }

        public List<SelectListItem> Roles { get; set; } = [];
    }
}
