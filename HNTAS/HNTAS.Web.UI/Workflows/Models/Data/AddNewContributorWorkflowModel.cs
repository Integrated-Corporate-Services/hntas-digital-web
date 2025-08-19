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
        public HeatNetworkInformationModel? HeatNetworkInformationModel { get; set; }
        public SelectRoleModel? SelectRoleModel { get; set; }

        public HashSet<ContributorWorkflowStep> CompletedSteps { get; set; } = new();

        public ContributorWorkflowStep CurrentStep { get; set; }

        public bool IsEmailStepValid() =>
            AddUserEmailAddressModel is { EmailAddress: var email } && !string.IsNullOrWhiteSpace(email);

        public void AdvanceToStep(ContributorWorkflowStep nextStep)
        {
            // Optional: enforce forward-only progression
            //if ((int)nextStep < (int)CurrentStep)
            //    throw new InvalidOperationException($"Cannot move backward from {CurrentStep} to {nextStep}.");

            CurrentStep = nextStep;
            CompletedSteps.Add(nextStep);
        }

        public object? GetStepData(ContributorWorkflowStep step) => step switch
        {
            ContributorWorkflowStep.AddEmailAddress => AddUserEmailAddressModel,
            ContributorWorkflowStep.ContactDetails => ContributorContactDetailsModel,
            ContributorWorkflowStep.ChooseHeatNetwork => HeatNetworkInformationModel,
            ContributorWorkflowStep.ChooseRole => SelectRoleModel,
            _ => null
        };

        void IWorkflowModel<ContributorWorkflowStep>.AdvanceToStep(ContributorWorkflowStep nextStep) => AdvanceToStep(nextStep);

        object? IWorkflowModel<ContributorWorkflowStep>.GetStepData(ContributorWorkflowStep step) => GetStepData(step);
    }


}
