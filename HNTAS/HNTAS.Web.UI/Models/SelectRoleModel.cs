using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class SelectRoleModel
    {
        [Required(ErrorMessage = "Choose the role.")]
        public string SelectedRoleId { get; set; } = null!;

        public List<SelectListItem> Roles { get; set; } = [];
    }
}
