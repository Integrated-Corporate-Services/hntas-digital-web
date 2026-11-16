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