using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Enums;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class NetworkElementsOverViewModel
    {
        public List<string>? ExistingNetworkElements { get; set; }
        public List<string>? NewNetworkElements { get; set; }
        public List<string>? ExistingNetworkEcNames { get; set; }
        public List<string>? NewNetworkEcNames { get; set; }
        public List<string>? ExistingEcSupplyingNewBuilding { get; set; }
        public string? Address { get; set; }
        public string? Coordinates { get; set; }
        public string? Phase { get; set; }
        public string? NetworkType { get; set; }
        public List<string>? Connections { get; set; }
    }    

    public class NetworkElementGroup
    {
        public HeatNetworkElementType ElementDisplayType { get; set; }
        public int? Count { get; set; }
        public List<SoaStages>? SoaStages { get; set; } = [];
        public string? ElementType { get; set; }
    }
}