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

        public static ElementSoaProgressStatusTracking GetElementSoaProgressStatusTracking(ElementSoaViewModel model)
        {
            var totalElementsInAllActiveStages = model.Stages.Where(w => w.IsActive).Sum(s => s.Elements.Count());
            var totalElementsWithStatusUpdated = model.Stages.Where(w => w.IsActive).Sum(s => s.Elements.Count(e => e.SoaStatus != null && e.SoaStatus != "Not started"));
            ElementSoaProgressStatusTracking incompleteSoa = new ElementSoaProgressStatusTracking();
            if (totalElementsInAllActiveStages > 0 && (totalElementsInAllActiveStages - totalElementsWithStatusUpdated) == 1)
            {
                incompleteSoa.AllElementsCompleted = false;
                // find the one stage and element that doesn't have a status
                var stageWithMissingDoc = model.Stages.FirstOrDefault(s => s.IsActive && s.Elements.Any(e => e.SoaStatus == null || e.SoaStatus == "Not started"));
                if (stageWithMissingDoc != null)
                {
                    incompleteSoa.IncompleteSoaStageId = stageWithMissingDoc.StageId;
                    var elementWithMissingDoc = stageWithMissingDoc.Elements.FirstOrDefault(e => e.SoaStatus == null || e.SoaStatus == "Not started");
                    if (elementWithMissingDoc != null)
                    {
                        incompleteSoa.IncompleteElementId = elementWithMissingDoc.ElementId;
                    }
                }
            }
            else if (totalElementsInAllActiveStages - totalElementsWithStatusUpdated == 0)
            {
                incompleteSoa.AllElementsCompleted = true;
            }
            return incompleteSoa;
        }

        public static (string Heading, string Description1, string Description2) GetSoaElementContent(HeatNetworkElementType? elementType)
        {
            return elementType switch
            {
                HeatNetworkElementType.EnergyCentre => (
                    "What is the status of the statement of applicability for the energy centre?",
                    "Intro to Energy centre...",
                    "Upload the statement of applicability (SOA) for the energy centre"
                ),
                HeatNetworkElementType.ConsumerConnection => (
                    "What is the status of the statement of applicability for the consumer connections",
                    "Intro to Consumer connections...",
                    "Upload the statement of applicability (SOA) for the consumer connections"
                ),
                HeatNetworkElementType.DistrictDistribution => (
                    "What is the status of the statement of applicability for the district distribution network",
                    "Intro to District distribution network...",
                    "Upload the statement of applicability (SOA) for the district distribution network"
                ),
                HeatNetworkElementType.Substation => (
                    "What is the status of the statement of applicability for the substation",
                    "Intro to substation",
                    "Upload the statement of applicability (SOA) for the substation"
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

        public static ElementSoaUpdateStatusViewModel GetSoaStatuses()
        {
            return new ElementSoaUpdateStatusViewModel
            {
                SoaStatus = new List<string> {
                    ElementSoaUpdateStatusConstants.InProgress,
                    ElementSoaUpdateStatusConstants.InRevision,
                    ElementSoaUpdateStatusConstants.CompletedSoaAndEvidenceWithAssessor,
                    ElementSoaUpdateStatusConstants.StatementOfApplicabilityAgreedWithAssessor,
                    ElementSoaUpdateStatusConstants.BeingAssessed }
            };
        }

        public static List<AssessmentOption> GetAssessmentOptions()
        {
            return new List<AssessmentOption>
            {
                new AssessmentOption
                {
                    Label = "Execute",
                    Hint = "Carry out the initial assessment and review the evidence provided"
                },
                new AssessmentOption
                {
                    Label = "Review",
                    Hint = "Check that the initial assessment was completed correctly"
                },
                new AssessmentOption
                {
                    Label = "Decision",
                    Hint = "Make the final decision on the outcome of the assessment"
                },
                new AssessmentOption
                {
                    Label = "Review and Decision",
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
        private static List<SoaElementsView> GetElementsForStage(List<Element>? elements)
        {
            var soaElements = new List<SoaElementsView>();
            foreach (var element in elements ?? [])
            {
                soaElements.Add(new SoaElementsView
                {
                    ElementId = element.ElementId,
                    Type = element.Type,
                    Name = element.NetworkElementInstanceName
                });
            }
            return soaElements;
        }
    }
}