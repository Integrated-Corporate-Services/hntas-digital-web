using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.Soa
{
    public class ElementListViewModel
    {
        public string HeatNetworkName { get; set; }
        public string HnId { get; set; } = null!;
        public List<ElementListItem> Elements { get; set; } = new();
    }

    public class ElementListItem
    {
        public HeatNetworkElementDisplayType ElementType { get; set; }

        public string Name { get; set; } = null!;

        public int Count { get; set; }

        public string UiStatus { get; set; } = UiStatusConstants.NotStarted; // e.g. "Completed", "In progress", "Cannot start yet"

        public bool IsEnabled { get; set; } // Whether the element is actionable in the UI
    }

    public static class UiStatusConstants
    {
        public const string Completed = "Completed";
        public const string CannotStartYet = "Cannot start yet";
        public const string NotStarted = "Not yet started";
        public const string InProgress = "In Progress";
    }
}
