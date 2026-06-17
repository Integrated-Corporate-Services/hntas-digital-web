using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.Soa;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Helpers
{
    public class StaticSoaSteps
    {
        public static List<StepNavItem> GetSteps(SoaSteps currentStep, IUrlHelper urlHelper)
        {
            var steps = new List<StepNavItem>
            {
                new() {
                    StepNumber = (int)SoaSteps.ChooseElements,
                    Title = "Choose your elements",
                    BodyContent = "You need to choose the elements from the list of 6 elements applicable for your heat network.",
                    LinkText = "Select your elements",
                    Url = urlHelper.Action("SelectElements", "Soa")
                },
                new() {
                    StepNumber =  (int)SoaSteps.InitialSoa,
                    Title = "Initial SOA",
                    BodyContent = "Share details on the selected elements for your heat network.",
                    LinkText = "Element details",
                    Url = urlHelper.Action("InitialSoa", "Soa")
                },
                new() {
                    StepNumber =  (int)SoaSteps.DefineSoa,
                    Title = "Define SOA",
                    BodyContent = "This is the content for writing well for the web.",
                    LinkText = "Define SOA",
                    Url = urlHelper.Action("DefineSoa", "Soa"),
                    IsCurrent = true,
                    IsExpanded = true
                },
                new() {
                    StepNumber = (int)SoaSteps.AddAssessmentPlan,
                    Title = "Add assessment plan",
                    BodyContent = "Add your assessment plan for the selected elements.",
                    LinkText = "Show",
                    Url = urlHelper.Action("AssessmentPlan", "Soa")
                },
                new() {
                    StepNumber = (int)SoaSteps.SubmitSoa,
                    Title = "Submit your SOA",
                    BodyContent = "Review and submit your completed SOA.",
                    LinkText = "Show",
                    Url = urlHelper.Action("SubmitSoa", "Soa")
                }
            };

            // Dynamically set IsCurrent and IsExpanded
            foreach (var step in steps)
            {
                step.IsCurrent = step.StepNumber == (int)currentStep;
                step.IsExpanded = step.StepNumber == (int)currentStep;
            }

            return steps;
        }
    }
}
