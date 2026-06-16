using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Workflows.Enums;

namespace HNTAS.Web.UI.Workflows.Models.Data
{
    public class AddNewContributorWorkflowModel : IWorkflowModel<ContributorWorkflowStep>
    {
        public AddUserEmailAddressModel? AddUserEmailAddressModel { get; set; }
        public ContributorContactDetailsModel? ContributorContactDetailsModel { get; set; }
        public ChooseHeatNetworkModel? ChooseHeatNetworkModel { get; set; }
        public ChooseRoleModel? ChooseRoleModel { get; set; }
        public ReplaceUserRoleViewModel? ReplaceUserRoleViewModel { get; set; }
        public HashSet<ContributorWorkflowStep> CompletedSteps { get; set; } = new();
        public ContributorWorkflowStep CurrentStep { get; set; }
        public void AdvanceToStep(ContributorWorkflowStep nextStep)
        {
            // Optional: enforce forward-only progression
            //if ((int)nextStep < (int)CurrentStep)
            //    throw new InvalidOperationException($"Cannot move backward from {CurrentStep} to {nextStep}.");

            CurrentStep = nextStep;
            CompletedSteps.Add(nextStep);
        }


        void IWorkflowModel<ContributorWorkflowStep>.AdvanceToStep(ContributorWorkflowStep nextStep) => AdvanceToStep(nextStep);

    }


}
