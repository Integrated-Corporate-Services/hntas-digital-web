using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class HeatNetworkConnectionsViewModel
    {
        public bool IsCommunalBuilding { get; set; }

        [Range(1, 9999, ErrorMessage = "Value must be between 1 and 9999")]
        public int? NoOfCommunalBuilding { get; set; }
        public bool IsDomesticConsumer { get; set; }

        [Range(1, 9999, ErrorMessage = "Value must be between 1 and 9999")]
        public int? NoOfDomesticConsumer { get; set; }
        public bool IsNonDomesticConsumer { get; set; }

        [Range(1, 9999, ErrorMessage = "Value must be between 1 and 9999")]
        public int? NoOfNonDomesticConsumer { get; set; }
        public bool IsDownstreamDistrictHeatNetworkConnections { get; set; }

        [Range(1, 9999, ErrorMessage = "Value must be between 1 and 9999")]
        public int? NoOfDownstreamDistrictHeatNetworkConnections { get; set; }        
    }
}
