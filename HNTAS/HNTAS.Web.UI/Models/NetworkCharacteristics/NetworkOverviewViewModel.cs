namespace HNTAS.Web.UI.Models.NetworkCharacteristics
{
    public class NetworkOverviewViewModel
    {
        public string HnId { get; set; }
        public string Status { get; set; } = "Cannot Start yet";
        public string SelectedHeatNetworkType { get; set; }
        public string? SelectedHeatGenerationSourceFor { get; set; }
        public int? NumberOfCommunalFloors { get; set; }
        public string? ContainsPressureBreak { get; set; }
        public List<string> NetworkSupply { get; set; }
    }
}
