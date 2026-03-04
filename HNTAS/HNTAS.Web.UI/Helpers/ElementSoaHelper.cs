using DocumentFormat.OpenXml.EMMA;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.ElementSoa;

namespace HNTAS.Web.UI.Helpers
{
    public class ElementSoaHelper
    {
        public static ElementSoaViewModel GetElementSoaViewModel(int eligibleIndex, int currentStageIndex, List<Element> networkElements)
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
                        Name = "Stage 2",
                        StageId = SoaStage.Stage2,
                        Elements = GetElementsForStage(networkElements),
                        IsActive = eligibleIndex == 0 || eligibleIndex == 1,
                        Title = "Design (Developed Design)"
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

        public static ElementSoaProgressStatusTracking GetElementSoaProgressStatusTracking(ElementSoaViewModel model)
        {
            var totalElementsInAllActiveStages = model.Stages.Where(w => w.IsActive).Sum(s => s.Elements.Count());
            var totalElementsWithDocuments = model.Stages.Where(w => w.IsActive).Sum(s => s.Elements.Count(e => e.Document != null));
            ElementSoaProgressStatusTracking incompleteSoa = new ElementSoaProgressStatusTracking();
            if (totalElementsInAllActiveStages > 0 && (totalElementsInAllActiveStages - totalElementsWithDocuments) == 1)
            {
                incompleteSoa.AllElementsCompleted = false;
                // find the one stage and element that doesn't have a document
                var stageWithMissingDoc = model.Stages.FirstOrDefault(s => s.IsActive && s.Elements.Any(e => e.Document == null));
                if (stageWithMissingDoc != null)
                {
                    incompleteSoa.IncompleteSoaStageId = stageWithMissingDoc.StageId;
                    var elementWithMissingDoc = stageWithMissingDoc.Elements.FirstOrDefault(e => e.Document == null);
                    if (elementWithMissingDoc != null)
                    {
                        incompleteSoa.IncompleteElementId = elementWithMissingDoc.ElementId;
                    }
                }
            }
            else if (totalElementsInAllActiveStages - totalElementsWithDocuments == 0)
            {
                incompleteSoa.AllElementsCompleted = true;
            }
            return incompleteSoa;
        }

        public static (string Heading, string Description1, string Description2) GetSoaElementContent(HeatNetworkElementDisplayType? elementType)
        {
            return elementType switch
            {
                HeatNetworkElementDisplayType.EnergyCentre => (
                    "Upload the statement of applicability (SOA) for the energy centre",
                    "Intro to Energy centre...",
                    "Upload the statement of applicability (SOA) for the energy centre"
                ),
                HeatNetworkElementDisplayType.DistributionNetwork => (
                    "Upload the statement of applicability (SOA) for the distribution network",
                    "Intro to Distribution network...",
                    "Upload the statement of applicability (SOA) for the distribution network"
                ),
                HeatNetworkElementDisplayType.ThermalSubStation => (
                    "Upload the statement of applicability (SOA) for the thermal substation",
                    "Intro to Thermal substation...",
                    "Upload the statement of applicability (SOA) for the thermal substation"
                ),
                HeatNetworkElementDisplayType.ConsumerConnections => (
                    "Upload the statement of applicability (SOA) for the consumer connections",
                    "Intro to Consumer connections...",
                    "Upload the statement of applicability (SOA) for the consumer connections"
                ),
                HeatNetworkElementDisplayType.CommunalDistributionNetwork => (
                    "Upload the statement of applicability (SOA) for the communal distribution network",
                    "Intro to Communal distribution network...",
                    "Upload the statement of applicability (SOA) for the communal distribution network"
                ),
                HeatNetworkElementDisplayType.ConsumerHeatSystems => (
                    "Upload the statement of applicability (SOA) for the consumer heat systems",
                    "Intro to Consumer heat systems...",
                    "Upload the statement of applicability (SOA) for the consumer heat systems"
                ),
                _ => (string.Empty, string.Empty, string.Empty)
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


        private static List<SoaElementsView> GetElementsForStage(List<Element>? elements)
        {
            var soaElements = new List<SoaElementsView>();
            foreach (var element in elements ?? [])
            {
                soaElements.Add(new SoaElementsView
                {
                    ElementId = element.ElementId,
                    //ElementType = element.ElementType,
                    Type = element.Type,
                    Name = Utility.GetDefaultNetworkElementOptions().Find(a => a.Id.ToString() == element.Type.ToString()).Label
                });
            }
            return soaElements;
        }
    }
}
