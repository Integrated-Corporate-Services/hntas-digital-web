using HNTAS.Api.Client.Model;
using System.Text.Json.Serialization;

namespace HNTAS.Web.UI.Models.EcPlantData
{
    public class ChpInformationViewModel
    {
        public List<ChpInformation> ChpInformationList { get; set; } = new List<ChpInformation>();
        
    }
    public class ChpInformation : HeatAndFuelBaseViewModel
    {
        public int? MaximumElectricOutput { get; set; }
        public string? OperationalMode { get; set; }        
    }
}
