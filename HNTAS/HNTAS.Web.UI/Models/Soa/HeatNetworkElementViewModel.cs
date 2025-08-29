using HNTAS.Web.UI.CustomValidation;

namespace HNTAS.Web.UI.Models.Soa
{
    public class HeatNetworkElementViewModel
    {
        public List<HeatNetworkElementOption> ElementOptions { get; set; } = new();

        [MustHaveOneItem(ErrorMessage = "Select at least one element that is part of your heat network.")]
        public List<string> SelectedElementIds { get; set; } = new();

        public Dictionary<string, int?> ElementCounts { get; set; } = new();
    }

    public class HeatNetworkElementOption
    {
        public string Id { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Hint { get; set; } = null!;
    }
}
