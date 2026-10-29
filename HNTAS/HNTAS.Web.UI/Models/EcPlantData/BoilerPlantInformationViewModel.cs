namespace HNTAS.Web.UI.Models.EcPlantData
{
    public class BoilerPlantInformationViewModel
    {
        public List<BoilerPlantInformation> BoilerPlantInformationList { get; set; } = new List<BoilerPlantInformation>();
    }

    public class BoilerPlantInformation : HeatAndFuelBaseViewModel
    {
    }
}
