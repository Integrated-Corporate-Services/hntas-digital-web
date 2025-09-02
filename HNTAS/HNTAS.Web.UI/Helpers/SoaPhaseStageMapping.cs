using HNTAS.Web.UI.Models.Soa;

namespace HNTAS.Web.UI.Helpers
{
    public static class SoaPhaseStageMapping
    {
        public static readonly List<PhaseDefinition> Phases = new()
    {
        new PhaseDefinition
        {
            Name = "Phase 1",
            Title = "Feasibility",
            Stages = new List<StageDefinition>
            {
                new() { Name = "Stage 1 – concept design" }
            }
        },
        new PhaseDefinition
        {
            Name = "Phase 2",
            Title = "Design",
            Stages = new List<StageDefinition>
            {
                new() { Name = "Stage 2 – developed design" },
                new() { Name = "Stage 3 – technical design" }
            }
        },
        new PhaseDefinition
        {
            Name = "Phase 3",
            Title = "Construction",
            Stages = new List<StageDefinition>
            {
                new() { Name = "Stage 4 – construction design" },
                new() { Name = "Stage 5 – installation" },
                new() { Name = "Stage 6 – commissioning" }
            }
        },
        new PhaseDefinition
        {
            Name = "Phase 4",
            Title = "Operation (initial 2 years)",
            Stages = new List<StageDefinition>
            {
                new() { Name = "Stage 7 – operation & maintenance" }
            }
        },
        new PhaseDefinition
        {
            Name = "Phase 5",
            Title = "Operation (ongoing)",
            Stages = new List<StageDefinition>
            {
                new() { Name = "Stage 8 – ongoing monitoring" }
            }
        }
    };
    }
}
