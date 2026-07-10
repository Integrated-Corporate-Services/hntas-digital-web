using HNTAS.Web.UI.Models.Address;

namespace HNTAS.Web.UI.Models.HeatNetwork
{
    public class HNDetailsViewModel
    {
        public string Name { get; set; }
        public string UHNID { get; set; }
        public string OrganisationName { get; set; }
        public AddressByStreetOrTownModel Address { get; set; }
        public string PathWay { get; set; }
        public string Phase { get; set; }
        public string? Coordinates { get; set; }
        public string? NetworkType { get; set; }
    }
}
