using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ExistingElementSoaViewModel
    {
        public NetworkDetailsStatus Status { get; set; }
        public List<SoaMilestonesView> Milestones { get; set; } = [];
        public int CurrentStageIndex { get; set; } = 0;
        public int EligibleStageIndex { get; set; } = 0;
    }

    public class SoaMilestonesView
    {
        public List<SoaElementsViewExistingNetwork> Elements { get; set; } = [];
        public bool IsActive { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public Milestone? MilestoneId { get; set; }
        public string? Description { get; set; }
    }

    public class SoaElementsViewExistingNetwork
    {
        public string? Name { get; set; }
        public HeatNetworkElementType? ElementDisplayType { get; set; }
        public string? ElementId { get; set; }
        public ElementTypeInShort? ElementType { get; set; }
        public string? SoaStatus { get; set; }
        public List<SoaStatusWithCountExistingNetwork>? SoaStatuses { get; set; }
        public DateTime? SoaStatusUpdatedAt { get; set; }
        public List<AssessorDetails> AssessorDetails { get; set; } = [];
    }
}
