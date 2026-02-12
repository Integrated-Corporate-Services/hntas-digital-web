using System.ComponentModel;

namespace HNTAS.Web.UI.Models.Enums
{
    public enum NetworkDetailsType
    {
        [Description("Network characteristics")]
        NetworkCharacteristics = 1,

        [Description("Network elements")]
        NetworkElements = 2,

        [Description("Soa")]
        Soa = 3,

        [Description("Metering and monitoring strategy")]
        MeteringAndMonitoringStrategy = 4,

        [Description("Assessment plan")]
        AssessmentPlan = 5,

        [Description("Design construction log")]
        DesignConstructionLog = 6
    }
}
