using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class ChooseContributorModel
    {
        [Required(ErrorMessage = "Please select a user.")]
        public string SelectedContributorId { get; set; } = null!;

        public string? SelectedContributorEmail { get; set; }

        public List<SelectListItem> Contributors { get; set; } = [];
    }
}
