using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.ElementSoa;

namespace HNTAS.Web.UI.Helpers
{
    public class ElementSoaHelper
    {
        public static ElementSoaViewModel GetElementSoaViewModel(int eligibleIndex, int currentStageIndex, List<ElementGroup> networkElements)
        {
            return new ElementSoaViewModel
            {
                EligibleStageIndex = eligibleIndex,
                Status = NetworkDetailsStatus.Incomplete,
                CurrentStageIndex = currentStageIndex,
                Stages = new List<SoaStagesView>
                {
                    new SoaStagesView
                    {
                        Name = "Stage 1",
                        StageId = SoaStage.Stage1,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = eligibleIndex == 0,
                        Title = "Feasibility (Concept Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 2 (optional)",
                        StageId = SoaStage.Stage2,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = eligibleIndex == 0 || eligibleIndex == 1,
                        Title = "Design (Developed Design)",
                        Description = "You may want to undertake a Stage 2 assessment to gain further assurance during your design development process, or if you want to provide an assessed design when handing over to your Design & Build Contractor."
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 3",
                        StageId = SoaStage.Stage3,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = true,
                        Title = "Design (Technical Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 4",
                        StageId = SoaStage.Stage4,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = true,
                        Title = "Construction (Construction Design)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 5",
                        StageId = SoaStage.Stage5,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = true,
                        Title = "Construction (Installation)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 6",
                        StageId = SoaStage.Stage6,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = true,
                        Title = "Construction (Commissioning)"
                    },
                    new SoaStagesView
                    {
                        Name = "Stage 7",
                        StageId = SoaStage.Stage7,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = true,
                        Title = "Operation (Operation and Maintenance)"
                    },

                }
            };
        }        

        public static string GetSoaElementContent(HeatNetworkElementType? elementType)
        {
            return elementType switch
            {
                HeatNetworkElementType.EnergyCentre => (
                    "What is the status of the statement of applicability for the Energy Centre?"
                ),
                HeatNetworkElementType.ConsumerConnection => (
                    "What is the status of the statement of applicability for the Consumer Connection?"
                ),
                HeatNetworkElementType.DistrictDistribution => (
                    "What is the status of the statement of applicability for the District Distribution Network?"
                ),
                HeatNetworkElementType.Substation => (
                    "What is the status of the statement of applicability for the Substation?"
                ),
                HeatNetworkElementType.CommunalDistribution => (
                    "What is the status of the statement of applicability for the Communal Distribution Network?"
                ),                
                _ => (string.Empty)
            };
        }

        public static int GetStageIndex(SoaStage stage)
        {
            return stage switch
            {
                SoaStage.Stage1 => 0,
                SoaStage.Stage2 => 1,
                SoaStage.Stage3 => 2,
                SoaStage.Stage4 => 3,
                SoaStage.Stage5 => 4,
                SoaStage.Stage6 => 5,
                SoaStage.Stage7 => 6,
                _ => 0
            };
        }
        
        public static List<SoaStatusOption> GetSoaStatuses()
        {
            return new List<SoaStatusOption>()
            {
                new() { Id = SoaStatus.NotStarted },
                new() { Id = SoaStatus.InProgress },
                new() { Id = SoaStatus.SoACompleted },
                new() { Id = SoaStatus.SoAAgreed },
                new() { Id = SoaStatus.BeingAssessed }
            };
        }

        public static List<SoaStatusWithCount> GetSoaStatuses(List<SoaStatusWithCount> soaStatuses)
        {
            var allStatuses = new List<SoaStatusWithCount>
            {
                new SoaStatusWithCount { SoaStatus = SoaStatus.NotStarted, Count = 0 },
                new SoaStatusWithCount { SoaStatus = SoaStatus.InProgress, Count = 0 },
                new SoaStatusWithCount { SoaStatus = SoaStatus.SoACompleted, Count = 0 },
                new SoaStatusWithCount { SoaStatus = SoaStatus.SoAAgreed, Count = 0 },
                new SoaStatusWithCount { SoaStatus = SoaStatus.BeingAssessed, Count = 0 }
            };
            if (soaStatuses == null || !soaStatuses.Any())
            {
                return allStatuses;
            }
            foreach (var status in allStatuses)
            {
                var matchingStatus = soaStatuses.FirstOrDefault(s => s.SoaStatus == status.SoaStatus);
                if (matchingStatus != null)
                {
                    status.Count = matchingStatus.Count;
                }
            }
            return allStatuses;
        }

        public static List<AssessmentOption> GetAssessmentOptions()
        {
            return new List<AssessmentOption>
            {
                new AssessmentOption
                {
                    Label = AssessmentConstants.Execute,
                    Hint = "Carry out the initial assessment and review the evidence provided"
                },
                new AssessmentOption
                {
                    Label = AssessmentConstants.Review,
                    Hint = "Check that the initial assessment was completed correctly"
                },
                new AssessmentOption
                {
                    Label = AssessmentConstants.Decision,
                    Hint = "Make the final decision on the outcome of the assessment"
                },
                new AssessmentOption
                {
                    Label = AssessmentConstants.ReviewAndDecision,
                    Hint = ""
                },
            };
        }

        public static string GetStageFromPhase(string phase)
        {
            return phase switch
            {
                "Feasibility" => "Concept design",
                "Design" => "Developed design, technical design",
                "Construction" => "Construction design, installation, commissioning",
                "Operational" => "Operation, maintenance, ongoing monitoring",
                _ => "NA"
            };
        }
        private static List<SoaElementsView> GetElementsForStage(List<ElementGroup>? elements)
        {
            var soaElements = new List<SoaElementsView>();
            foreach (var element in elements ?? new List<ElementGroup>())
            {
                // Convert NullableOfHeatNetworkElementType? to HeatNetworkElementType? for comparison and lookup
                HeatNetworkElementType? elementType = element.ElementDisplayType.HasValue
                    ? (HeatNetworkElementType)(int)element.ElementDisplayType.Value
                    : (HeatNetworkElementType?)null;

                soaElements.Add(new SoaElementsView
                {                    
                    ElementType = element.ElementType,
                    ElementDisplayType = elementType,                    
                    Name = $"{ NetworkElementHelper.GetNetworkElementOptionsForNetworkType().FirstOrDefault(n => n.Id == elementType)?.Label }{(element.Count > 1 ? $" ({element.Count})" : string.Empty)}"
                });
            }
            return soaElements;
        }
    }
}