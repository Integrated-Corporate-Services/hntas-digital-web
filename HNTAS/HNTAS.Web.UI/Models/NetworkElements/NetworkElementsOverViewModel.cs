using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Address;
using System.ComponentModel.DataAnnotations;

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
        public HeatNetworkElementDisplayType Type { get; set; }
        public int? Count { get; set; }
    }
}