using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ElementSoaProgressStatusTracking
    {
        public string? IncompleteElementId { get; set; }
        public SoaStage? IncompleteSoaStageId { get; set; }
        public bool AllElementsCompleted { get; set; }
    }
}
