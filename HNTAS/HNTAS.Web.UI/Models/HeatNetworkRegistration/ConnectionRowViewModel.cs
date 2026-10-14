namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class ConnectionRowViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string ExistingProperty { get; set; } = string.Empty;
        public int? ExistingValue { get; set; }

        public string NewProperty { get; set; } = string.Empty;
        public int? NewValue { get; set; }

        public bool ShowExisting { get; set; }
        public bool ShowNew { get; set; }
    }
}
