using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.EcPlantData
{
    public class ChpInformationViewModel
    {
        public List<ChpInformation> ChpInformationList { get; set; } = new List<ChpInformation>();
        public List<CHPOperationalMode>? OperationalModes { get; set; } = new List<CHPOperationalMode>();
        public List<FuelType>? FuelTypes { get; set; } = new List<FuelType>();
    }
    public class ChpInformation : HeatAndFuelBaseViewModel
    {
        public int? MaximumElectricOutput { get; set; }
        public string? OperationalModeValue { get; set; }        
    }
}
