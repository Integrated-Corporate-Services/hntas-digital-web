using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models
{
    public class HeatNetworksViewModel
    {
        public List<HeatNetworkModel> HeatNetworks { get; set; } = new List<HeatNetworkModel>();

        public bool IsResponsiblePerson { get; set; }

        public bool IsHntasCoordinator { get; set; }
    }

    public class HeatNetworkModel
    {
        public string HnId { get; set; }
        public string Name { get; set; }
        public string OrganisationName { get; set; }
        public string Role { get; set; }
    }
}
