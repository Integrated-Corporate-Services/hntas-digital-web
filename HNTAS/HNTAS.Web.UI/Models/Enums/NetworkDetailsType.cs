using System.ComponentModel;

namespace HNTAS.Web.UI.Models.Enums
{
    public enum NetworkDetailsType
    {        

        [Description("Network elements")]
        NetworkElements = 1,

        [Description("Soa")]
        Soa = 2,

        [Description("Metering and monitoring strategy")]
        MeteringAndMonitoringStrategy = 3,

        [Description("Assessment plan")]
        AssessmentPlan = 4,

        [Description("Design construction log")]
        DesignConstructionLog = 5
    }
}
