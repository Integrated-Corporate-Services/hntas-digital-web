using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class YouHaveBeenInvitedModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string AcceptInvitation { get; set; }
    }
    public class DHDashboardModel
    {
        [Required(ErrorMessage = "Details for the duty holder is missing.")]
        public string OrganisationName { get; set; }

        [Required(ErrorMessage = "Details for the duty holder is missing.")]
        public string HeatNetwork { get; set; }

        public string? HNStatus { get; set; }
    }
}
