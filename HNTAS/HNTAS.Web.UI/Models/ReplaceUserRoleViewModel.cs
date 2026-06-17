using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class ReplaceUserRoleViewModel
    {
        [Required(ErrorMessage = "Please select a valid option.")]
        public string ReplaceExistingRole { get; set; } = null!;
        public string? CurrentRoleUserId { get; set; }
        public string? HeatNetworkName { get; set; }
        public string? RoleName { get; set; }
    }
}