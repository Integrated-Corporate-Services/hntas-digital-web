using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Soa
{
    public class EnterElementLocationsViewModel
    {
        public string? ElementName { get; set; }

        [MinLength(1, ErrorMessage = "At least one location must be provided.")]
        public List<string> Locations { get; set; } = new();
    }
}
