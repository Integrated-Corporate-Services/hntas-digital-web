using HNTAS.Web.UI.Models.Address;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{ 
    public class NetworkElementsOverviewModel
    {
        public AddressByStreetOrTownModel? HeatNetworkAddressModel { get; set; }
        public ECDetailsModel ECDetailsModel { get; set; }
    }
}
