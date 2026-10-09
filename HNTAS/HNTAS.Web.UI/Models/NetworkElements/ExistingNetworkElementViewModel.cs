using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class ExistingNetworkElementViewModel
    {
        public List<ExistingNetworkElementOption> ElementOptions { get; set; } = new();
    }

    public class ExistingNetworkElementOption
    {
        public HeatNetworkElementType Id { get; set; }
        public string? Label { get; set; } = null;
        public string? SubLabel { get; set; } = null;
        public string? Hint { get; set; } = null;
        public int? ExistingCount { get; set; } = null;
        public int? NewCount { get; set; } = null;
    }
}
