using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class HeatNetworkConnectionsViewModel
    {
        public bool IsCommunalBuilding { get; set; }
        public int? NoOfCommunalBuilding { get; set; }
        public bool IsDomesticConsumer { get; set; }
        public int? NoOfDomesticConsumer { get; set; }
        public bool IsNonDomesticConsumer { get; set; }
        public int? NoOfNonDomesticConsumer { get; set; }
        public bool IsUpstreamDistrictHeatNetworkConnections { get; set; }
        public int? NoOfUpstreamDistrictHeatNetworkConnections { get; set; }
    }
}
