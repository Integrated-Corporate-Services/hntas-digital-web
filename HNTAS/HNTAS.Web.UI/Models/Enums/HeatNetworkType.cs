using System.ComponentModel;

namespace HNTAS.Web.UI.Models.Enums
{
    public enum HeatNetworkType
    {
        [Description("Communal (with an integral energy centre)")]
        CommunalWithIntegralEC = 1,

        [Description("Communal (supplied by a separate upstream heat network)")]
        CommunalWithSeparateUpstreamHN = 2,

        [Description("District (with its own main energy centre)")]
        DistrictWithOwnEC = 3,

        [Description("District (supplied by a separate upstream heat network)")]
        DistrictWithSeparateUpstreamHN = 4,
    }

    public static class HeatNetworkTypeConstants
    {
        public const string CommunalWithIntegralEC = "Communal (with an integral energy centre)";
        public const string CommunalWithSeparateUpstreamHN = "Communal (supplied by a separate upstream heat network)";
        public const string DistrictWithOwnEC = "District (with its own main energy centre)";
        public const string DistrictWithSeparateUpstreamHN = "District (supplied by a separate upstream heat network)";
        public const string Communal = "Communal";
        public const string District = "District";
    }
}
