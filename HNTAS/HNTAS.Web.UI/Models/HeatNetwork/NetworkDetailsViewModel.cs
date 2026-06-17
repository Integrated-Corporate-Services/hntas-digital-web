using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Enums;

namespace HNTAS.Web.UI.Models.HeatNetwork
{
    public class NetworkDetailsViewModel
    {
        public List<NetworkDetailsOption> DetailsOptions { get; set; } = new();
    }

    public class NetworkDetailsOption
    {
        public NetworkDetailsType Id { get; set; }
        public string Label { get; set; } = null!;
        public string Hint { get; set; } = null!;
        public string UiStatus { get; set; } = StatusConstants.NotStarted;
        public bool IsEnabled { get; set; }
    }
}
