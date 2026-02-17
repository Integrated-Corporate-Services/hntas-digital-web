using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Address;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{ 
    public class NetworkElementsOverviewModel
    {
        public List<Element> Elements { get; set; } = new List<Element>();
        public AddressByStreetOrTownModel? HeatNetworkAddressModel { get; set; }
        public ECDetailsModel? ECDetailsModel { get; set; }
    }

    public class Element
    {
        public HeatNetworkElementType Type { get; set; }
        public int? Count { get; set; }
    }
}
