using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class NetworkStatusModel
    {
        [Required(ErrorMessage = "Select the build status of your network")]
        public string? BuildStatus { get; set; }
    }
}
