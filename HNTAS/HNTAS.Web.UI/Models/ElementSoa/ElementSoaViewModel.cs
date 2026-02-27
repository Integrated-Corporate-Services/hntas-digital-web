using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ElementSoaViewModel
    {
        public NetworkDetailsStatus Status { get; set; }
        public List<SoaStagesView> Stages { get; set; } = [];
        public int CurrentStageIndex { get; set; } = 0;
    }

    public class SoaStagesView : SoaStages
    {
        public new List<SoaElementsView> Elements { get; set; } = [];
        public bool IsActive { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
    }

    public class SoaElementsView : Elements
    {
        public string? Name { get; set; }
    }
}
