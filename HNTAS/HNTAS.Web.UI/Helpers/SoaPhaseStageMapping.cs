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
                    new() { Name = "Stage 1 – concept design", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage1 }
                }
            },
            new PhaseDefinition
            {
                Name = "Phase 2",
                Title = "Design",
                Stages = new List<StageDefinition>
                {
                    new() { Name = "Stage 2 – developed design", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage2 },
                    new() { Name = "Stage 3 – technical design", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage3 }
                }
            },
            new PhaseDefinition
            {
                Name = "Phase 3",
                Title = "Construction",
                Stages = new List<StageDefinition>
                {
                    new() { Name = "Stage 4 – construction design", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage4},
                    new() { Name = "Stage 5 – installation", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage5 },
                    new() { Name = "Stage 6 – commissioning", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage6 }
                }
            },
            new PhaseDefinition
            {
                Name = "Phase 4",
                Title = "Operation (initial 2 years)",
                Stages = new List<StageDefinition>
                {
                    new() { Name = "Stage 7 – operation & maintenance", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage7 }
                }
            },
            new PhaseDefinition
            {
                Name = "Phase 5",
                Title = "Operation (ongoing)",
                Stages = new List<StageDefinition>
                {
                    new() { Name = "Stage 8 – ongoing monitoring", SoaStage = Api.Client.Model.NullableOfSoaStage.Stage8}
                }
            }
        };
    }
}
