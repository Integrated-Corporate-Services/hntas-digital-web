using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class AddContributorViewModel
    {
        [Required(ErrorMessage = "Select one option")]
        public bool? InviteNewContributor { get; set; }
    }
}
