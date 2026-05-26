using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorRoleViewModel
    {
        [Required(ErrorMessage = "Select the user role")]
        public bool? IsDDH { get; set; }
    }
}
