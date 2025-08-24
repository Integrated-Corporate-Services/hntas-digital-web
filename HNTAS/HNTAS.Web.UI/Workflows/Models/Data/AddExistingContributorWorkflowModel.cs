using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Workflows.Enums;

namespace HNTAS.Web.UI.Workflows.Models.Data
{
    public class AddExistingContributorWorkflowModel : IWorkflowModel<ExistingContributorWorkflowStep>
    {
        public ChooseContributorModel? ChooseContributorModel { get; set; }
        public ContributorContactDetailsModel? ContributorContactDetailsModel { get; set; }
        public ChooseHeatNetworkModel? ChooseHeatNetworkModel { get; set; }
        public ChooseRoleModel? ChooseRoleModel { get; set; }
        public HashSet<ExistingContributorWorkflowStep> CompletedSteps { get; set; } = new();
        public ExistingContributorWorkflowStep CurrentStep { get; set; }

        public void AdvanceToStep(ExistingContributorWorkflowStep nextStep)
        {
            CurrentStep = nextStep;
            CompletedSteps.Add(nextStep);
        }

        void IWorkflowModel<ExistingContributorWorkflowStep>.AdvanceToStep(ExistingContributorWorkflowStep nextStep) => AdvanceToStep(nextStep);
    }
}
