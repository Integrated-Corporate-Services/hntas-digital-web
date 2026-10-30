using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class HeatNetworkConnectionSummaryViewModel
    {
        public NetworkConnectionsViewModel Connections { get; set; } = new();
        public CommunalBuildingResponsibilityViewModel? Responsibility { get; set; }
        public NetworkStatusModel? NetworkStatus { get; set; }

        public string EnergyCentreOption { get; set; } = string.Empty;

        public bool ShowExistingSection => Connections.ShowExistingColumn && HasAnyExistingConnections;
        public bool ShowNewSection => Connections.ShowNewColumn && HasAnyNewConnections;

        public bool HasAnyExistingConnections =>
            (Connections.ExistingCommunalBuildings ?? 0) > 0 ||
            (Connections.ExistingHousesOrMaisonettes ?? 0) > 0 ||
            (Connections.ExistingNonDomesticBuildings ?? 0) > 0 ||
            (Connections.ExistingMultiTenantedNonDomestic ?? 0) > 0 ||
            (Connections.ExistingOtherDistrictNetworks ?? 0) > 0;

        public bool HasAnyNewConnections =>
            (Connections.NewCommunalBuildings ?? 0) > 0 ||
            (Connections.NewHousesOrMaisonettes ?? 0) > 0 ||
            (Connections.NewNonDomesticBuildings ?? 0) > 0 ||
            (Connections.NewMultiTenantedNonDomestic ?? 0) > 0 ||
            (Connections.NewOtherDistrictNetworks ?? 0) > 0;
    }
}
