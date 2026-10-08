using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.ElementSoa;

namespace HNTAS.Web.UI.Helpers
{
    public class ExistingElementSoaHelper
    {
        public static ExistingElementSoaViewModel GetElementSoaViewModel(int eligibleIndex, int currentStageIndex, List<ElementGroup> networkElements, HeatNetworkType? networkType, bool hasOwnEc)
        {
            return new ExistingElementSoaViewModel
            {
                EligibleStageIndex = eligibleIndex,
                Status = NetworkDetailsStatus.Incomplete,
                CurrentStageIndex = currentStageIndex,
                Milestones = new List<SoaMilestonesView>
                {
                    new SoaMilestonesView
                    {
                        Name = "Milestone 2",
                        MilestoneId = Milestone.Milestone2,
                        Elements = GetElementsForStage(networkElements, Milestone.Milestone2, networkType, hasOwnEc),
                        IsActive = eligibleIndex == 0,
                        Title = "Metering and monitoring (threshold performance)"
                    },
                    new SoaMilestonesView
                    {
                        Name = "Milestone 3A",
                        MilestoneId = Milestone.Milestone3A,
                        Elements = GetElementsForStage(networkElements, Milestone.Milestone3A, networkType, hasOwnEc),
                        IsActive = eligibleIndex == 0 || eligibleIndex == 1,
                        Title = "Performance improvement plan",
                        Description = "You may want to undertake a Stage 2 assessment to gain further assurance during your design development process, or if you want to provide an assessed design when handing over to your Design & Build Contractor."
                    },
                    new SoaMilestonesView
                    {
                        Name = "Milestone 3B",
                        MilestoneId = Milestone.Milestone3B,
                        Elements = GetElementsForStage(networkElements, Milestone.Milestone3B, networkType, hasOwnEc),
                        IsActive = true,
                        Title = "Metering and monitoring (end-user connections)",
                        Description = "Only applies to consumer connections"
                    },
                    new SoaMilestonesView
                    {
                        Name = "Milestone 4",
                        MilestoneId = Milestone.Milestone4,
                        Elements = GetElementsForStage(networkElements, Milestone.Milestone4, networkType, hasOwnEc),
                        IsActive = true,
                        Title = "TBD"
                    },
                    new SoaMilestonesView
                    {
                        Name = "Milestone 5",
                        MilestoneId = Milestone.Milestone5,
                        Elements = GetElementsForStage(networkElements, Milestone.Milestone5, networkType, hasOwnEc),
                        IsActive = true,
                        Title = "TBD"
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

        public static int GetMilestoneIndex(Milestone milestone)
        {
            return milestone switch
            {
                Milestone.Milestone2 => 0,
                Milestone.Milestone3A => 1,
                Milestone.Milestone3B => 2,
                Milestone.Milestone4 => 3,
                Milestone.Milestone5 => 4,
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
        private static List<SoaElementsViewExistingNetwork> GetElementsForStage(List<ElementGroup>? elements, Milestone milestone, HeatNetworkType? networkType = null, bool hasOwnEc = false)
        {
            var soaElements = new List<SoaElementsViewExistingNetwork>();
            foreach (var element in elements ?? new List<ElementGroup>())
            {
                if (milestone == Milestone.Milestone3B && element.ElementType != ElementTypeInShort.CC)
                    continue;

                HeatNetworkElementType? elementType = element.ElementDisplayType.HasValue
                    ? (HeatNetworkElementType)(int)element.ElementDisplayType.Value
                    : (HeatNetworkElementType?)null;

                var elementDisplayName = string.Empty;
                var el = NetworkElementHelper.GetNetworkElementOptionsForNetworkType().FirstOrDefault(n => n.Id == elementType);
                if (networkType == HeatNetworkType.District && !hasOwnEc)
                {
                    elementDisplayName = $"{el?.Label}{el?.Hint}{(element.Count > 1 ? $" ({element.Count})" : string.Empty)}";
                }
                else
                {
                    elementDisplayName = $"{el?.Label}{(element.Count > 1 ? $" ({element.Count})" : string.Empty)}";
                }
                soaElements.Add(new SoaElementsViewExistingNetwork
                {
                    ElementType = element.ElementType,
                    ElementDisplayType = elementType,
                    Name = elementDisplayName
                });
            }
            return soaElements;
        }
    }
}