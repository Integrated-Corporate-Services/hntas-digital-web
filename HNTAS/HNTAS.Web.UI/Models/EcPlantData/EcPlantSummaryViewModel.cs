namespace HNTAS.Web.UI.Models.EcPlantData
{
    public class EcPlantSummaryViewModel
    {
        public List<ChpInformationSummary>? ChpInformationSummaryList { get; set; }
        public List<HeatPumpInformationSummary>? HeatPumpInformationSummaryList { get; set; }
        public List<HighHeatRecoveryInformationSummary>? HighHeatRecoveryInformationSummaryList { get; set; }
        public List<BoilerPlantInformationSummary>? BoilerPlantInformationSummaryList { get; set; }
    }

    public class ChpInformationSummary
    {
        public string? MaximumElectricOutput { get; set; }
        public string? OperationalModeValue { get; set; }
        public string? MaximumHeatOutput { get; set; }
        public string? FuelOrHeatSourceValue { get; set; }
    }
    public class HeatPumpInformationSummary
    {
        public string? MaximumHeatOutput { get; set; }
        public string? FuelOrHeatSourceValue { get; set; }
    }
    public class HighHeatRecoveryInformationSummary
    {
        public string? MaximumHeatOutput { get; set; }
        public string? FuelOrHeatSourceValue { get; set; }
    }
    public class BoilerPlantInformationSummary
    {
        public string? MaximumHeatOutput { get; set; }
        public string? FuelOrHeatSourceValue { get; set; }
    }
}
