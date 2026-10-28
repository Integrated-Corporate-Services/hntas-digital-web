namespace HNTAS.Web.UI.Models.EcPlantData
{
    public class HighHeatRecoveryInformationViewModel
    {
        public List<HighHeatRecoveryInformation> HighHeatRecoveryInformationList { get; set; } = new List<HighHeatRecoveryInformation>();
    }

    public class HighHeatRecoveryInformation
    {
        public int? MaximumHeatOutput { get; set; }
        public string? FuelOrHeatSource { get; set; }
    }
}
