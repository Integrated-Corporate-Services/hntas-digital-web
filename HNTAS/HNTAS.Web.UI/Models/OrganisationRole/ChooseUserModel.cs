using HNTAS.Web.UI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.OrganisationRole
{
    public class ChooseUserModel
    {
        [Required(ErrorMessage = "Please select a user.")]
        public string SelectedUserId { get; set; } = null!;

        public string? SelectedUserName { get; set; }

        public List<SelectItemOption> Users { get; set; } = [];
    }
}
