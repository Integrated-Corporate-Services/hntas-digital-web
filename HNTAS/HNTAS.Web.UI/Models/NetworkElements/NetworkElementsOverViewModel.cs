using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class NetworkElementsOverViewModel
    {
        public List<string> Elements { get; set; } = new List<string>();
        public string? HeatNetworkAddress { get; set; }
        public string? Coordinates { get; set; }
        public string NetworkType { get; set; } = null!;
        public string Phase { get; set; } = null!;
    }

    public class Element
    {
        public HeatNetworkElementType Type { get; set; }
        public int? Count { get; set; }
        //public string? NetworkElementInstanceName { get; set; }
    }
}