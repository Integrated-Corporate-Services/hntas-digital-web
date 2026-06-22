using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models
{
    public class ExistingNetworksViewModel
    {
        public List<ExistingNetworkModel> HeatNetworks { get; set; } = new List<ExistingNetworkModel>();

        public bool IsResponsiblePerson { get; set; }

        public bool IsHntasCoordinator { get; set; }
    }

    public class ExistingNetworkModel
    {
        public string HnId { get; set; }
        public string Name { get; set; }
        public string? HnDescription { get; set; }
        public string OrganisationName { get; set; }
        public string Role { get; set; }
        public DateTime? OfgemImportedDate { get; set; }
    }
}
