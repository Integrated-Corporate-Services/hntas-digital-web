using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.EcPlantData
{
    public class HeatPumpInformationViewModel
    {
        public List<HeatPumpInformation> HeatPumpInformationList { get; set; } = new List<HeatPumpInformation>();        
    }

    public class HeatPumpInformation : HeatAndFuelBaseViewModel
    {        
    }
}
