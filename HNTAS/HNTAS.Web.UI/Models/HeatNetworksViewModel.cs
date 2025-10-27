namespace HNTAS.Web.UI.Models
{
    public class HeatNetworksViewModel
    {
        public List<HeatNetworkModel> HeatNetworks { get; set; } = new List<HeatNetworkModel>();

        public bool IsRegulatoryContact { get; set; }
    }

    public class HeatNetworkModel
    {
        public string HnId { get; set; }
        public string Name { get; set; }
        public string OrganisationName { get; set; }
        public string Status { get; set; } // Default
    }
}
