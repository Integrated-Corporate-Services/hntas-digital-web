using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ElementSoaViewModel
    {
        public NetworkDetailsStatus Status { get; set; }
        public List<SoaStagesView> Stages { get; set; } = [];
        public int CurrentStageIndex { get; set; } = 0;
        public int EligibleStageIndex { get; set; } = 0;
    }

    public class SoaStagesView
    {
        public List<SoaElementsView> Elements { get; set; } = [];
        public bool IsActive { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public SoaStage? StageId { get; set; }
        public string? Description { get; set; }
    }

    public class SoaElementsView
    {
        public string? Name { get; set; }
        public HeatNetworkElementType? ElementDisplayType { get; set; }
        public string? ElementId { get; set; }
        public ElementTypeInShort? ElementType { get; set; }
        public string? SoaStatus { get; set; }
        public List<SoaStatusWithCount>? SoaStatuses { get; set; }
        public DateTime? SoaStatusUpdatedAt { get; set; }
        //public string? AssessorEmailId { get; set; }
        //public string? AssessorFirstName { get; set; }
        //public string? AssessorLastName { get; set; }
        //public string? Assessment { get; set; }
        public List<AssessorDetails> AssessorDetails { get; set; } = [];
    }
    
}
