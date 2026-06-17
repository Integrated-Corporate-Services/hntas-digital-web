using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class ExistingContributorsListViewModel
    {
        public List<NewContributorDetailsViewModel> Contributors { get; set; } = new List<NewContributorDetailsViewModel>();
        [Required(ErrorMessage = "Select a user to invite")]
        public string SelectedEmailAddress { get; set; }
        public NewContributorDetailsViewModel? SelectedUser { get; set; }
    }
}
