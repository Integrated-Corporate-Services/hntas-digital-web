using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class NetworkConnectionsViewModel
    {
        // UI Display Flags
        public bool ShowExistingColumn { get; set; }
        public bool ShowNewColumn { get; set; }
        public bool ShowOtherDistrictNetworksRow { get; set; }

        // --- Existing Network Connections ---
        [Required(ErrorMessage = "Enter number of existing communal buildings")]
        [Range(0, 99999, ErrorMessage = "Number of existing communal buildings must be 0 or more")]
        public int? ExistingCommunalBuildings { get; set; }

        [Required(ErrorMessage = "Enter number of existing houses or maisonettes")]
        [Range(0, 99999, ErrorMessage = "Number of existing houses or maisonettes must be 0 or more")]
        public int? ExistingHousesOrMaisonettes { get; set; }

        [Required(ErrorMessage = "Enter number of existing non-domestic buildings")]
        [Range(0, 99999, ErrorMessage = "Number of existing non-domestic buildings must be 0 or more")]
        public int? ExistingNonDomesticBuildings { get; set; }

        [Required(ErrorMessage = "Enter number of existing multi-tenanted non-domestic buildings")]
        [Range(0, 99999, ErrorMessage = "Number of existing multi-tenanted non-domestic buildings must be 0 or more")]
        public int? ExistingMultiTenantedNonDomestic { get; set; }

        [Required(ErrorMessage = "Enter number of existing other district heat networks supplied")]
        [Range(0, 99999, ErrorMessage = "Number of existing other district heat networks supplied must be 0 or more")]
        public int? ExistingOtherDistrictNetworks { get; set; }

        // --- New Network Connections ---
        [Required(ErrorMessage = "Enter number of new communal buildings")]
        [Range(0, 99999, ErrorMessage = "Number of new communal buildings must be 0 or more")]
        public int? NewCommunalBuildings { get; set; }

        [Required(ErrorMessage = "Enter number of new houses or maisonettes")]
        [Range(0, 99999, ErrorMessage = "Number of new houses or maisonettes must be 0 or more")]
        public int? NewHousesOrMaisonettes { get; set; }

        [Required(ErrorMessage = "Enter number of new non-domestic buildings")]
        [Range(0, 99999, ErrorMessage = "Number of new non-domestic buildings must be 0 or more")]
        public int? NewNonDomesticBuildings { get; set; }

        [Required(ErrorMessage = "Enter number of new multi-tenanted non-domestic buildings")]
        [Range(0, 99999, ErrorMessage = "Number of new multi-tenanted non-domestic buildings must be 0 or more")]
        public int? NewMultiTenantedNonDomestic { get; set; }

        [Required(ErrorMessage = "Enter number of new other district heat networks supplied")]
        [Range(0, 99999, ErrorMessage = "Number of new other district heat networks supplied must be 0 or more")]
        public int? NewOtherDistrictNetworks { get; set; }

        public List<string> GetExistingConnectionsToDisplay()
        {
            var list = new List<string>();
            if (ExistingCommunalBuildings.HasValue && ExistingCommunalBuildings > 0)
                list.Add($"{ExistingCommunalBuildings} communal buildings");
            if (ExistingHousesOrMaisonettes.HasValue && ExistingHousesOrMaisonettes > 0)
                list.Add($"{ExistingHousesOrMaisonettes} houses or maisonettes");
            if (ExistingNonDomesticBuildings.HasValue && ExistingNonDomesticBuildings > 0)
                list.Add($"{ExistingNonDomesticBuildings} non-domestic buildings");
            if (ExistingMultiTenantedNonDomestic.HasValue && ExistingMultiTenantedNonDomestic > 0)
                list.Add($"{ExistingMultiTenantedNonDomestic} multi-tenanted non-domestic buildings");
            if (ExistingOtherDistrictNetworks.HasValue && ExistingOtherDistrictNetworks > 0)
                list.Add($"{ExistingOtherDistrictNetworks} district networks supplied");
            return list;
        }

        public List<string> GetNewConnectionsToDisplay()
        {
            var list = new List<string>();
            if (NewCommunalBuildings.HasValue && NewCommunalBuildings > 0)
                list.Add($"{NewCommunalBuildings} communal buildings");
            if (NewHousesOrMaisonettes.HasValue && NewHousesOrMaisonettes > 0)
                list.Add($"{NewHousesOrMaisonettes} houses or maisonettes");
            if (NewNonDomesticBuildings.HasValue && NewNonDomesticBuildings > 0)
                list.Add($"{NewNonDomesticBuildings} non-domestic buildings");
            if (NewMultiTenantedNonDomestic.HasValue && NewMultiTenantedNonDomestic > 0)
                list.Add($"{NewMultiTenantedNonDomestic} multi-tenanted non-domestic buildings");
            if (NewOtherDistrictNetworks.HasValue && NewOtherDistrictNetworks > 0)
                list.Add($"{NewOtherDistrictNetworks} district networks supplied");
            return list;
        }
    }
}
