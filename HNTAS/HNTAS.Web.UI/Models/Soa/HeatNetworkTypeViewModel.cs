using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Soa
{
    public class HeatNetworkTypeViewModel
    {
        // This list will hold the display text and value for each radio button
        public List<SelectItemOption> HeatNetworkTypes { get; set; } = new List<SelectItemOption>();

        [Required(ErrorMessage = "Select which type of organisation you work for")]
        public string? SelectedHNType { get; set; }

        public string? OtherNetworkDescription { get; set; }
    }

    public class SelectItemOption
    {
        public string Value { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string? Hint { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
