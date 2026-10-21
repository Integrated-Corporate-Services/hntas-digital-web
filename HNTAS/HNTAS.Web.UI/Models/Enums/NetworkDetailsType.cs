using System.ComponentModel;

namespace HNTAS.Web.UI.Models.Enums
{
    public enum NetworkDetailsType
    {
        [Description("Network elements")]
        NetworkElements = 1,

        [Description("Soa")]
        Soa = 2,

        [Description("Energy centre plant data")]
        EnergyCentrePlantData = 3        
    }
}
