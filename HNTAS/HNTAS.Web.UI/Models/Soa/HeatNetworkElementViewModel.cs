using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.CustomValidation;

namespace HNTAS.Web.UI.Models.Soa
{
    public class HeatNetworkElementOption
    {
        public HeatNetworkElementType Id { get; set; }
        public string Label { get; set; } = null!;
        public string Hint { get; set; } = null!;
    }
}
