using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.CustomValidation;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class NetworkElementViewModel
    {
        public List<NetworkElementOption> ElementOptions { get; set; } = new();

        [MustHaveOneItem(ErrorMessage = "Select at least one element that is part of your heat network.")]
        public List<HeatNetworkElementType> SelectedElementIds { get; set; } = new();
        public Dictionary<HeatNetworkElementType, int?> ElementCounts { get; set; } = new();
    }

    public class NetworkElementOption
    {
        public HeatNetworkElementType Id { get; set; }
        public string Label { get; set; } = null!;
        public string SubLabel { get; set; } = null!;
        public string Hint { get; set; } = null!;
    }

    public class AssessorSelectElementsViewModel
    {
        public List<AssessorSelectElementsOption>? ElementOptions { get; set; } = new();
        [MustHaveOneItem(ErrorMessage = "Select at least one element before continuing.")]
        public List<string>? SelectedElementIds { get; set; } = new();
        public List<string>? SelectedElementLabel { get; set; } = new();
    }

    public class AssessorSelectElementsOption
    {
        public string Label { get; set; } = null!;
        public string ElementId { get; set; } = null!;
        public string AssignedAssessorName { get; set; } = null!;
    }
}
