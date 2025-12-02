using HNTAS.Web.UI.Models.OrganisationRole;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Workflows.Enums;

namespace HNTAS.Web.UI.Workflows.Models.Data
{
    public class AddOrganisationUserWorkflowModel : IWorkflowModel<AddOrganisationUserWorkflowStep>
    {
        public AddUserEmailAddressModel? AddUserEmailAddressModel { get; set; }
        public ContributorContactDetailsModel? ContributorContactDetailsModel { get; set; }
        public RoleAssignmentModel? RoleAssignmentModel { get; set; }


        public HashSet<AddOrganisationUserWorkflowStep> CompletedSteps { get; set; } = new();
        public AddOrganisationUserWorkflowStep CurrentStep { get; set; }

        public void AdvanceToStep(AddOrganisationUserWorkflowStep nextStep)
        {
            CurrentStep = nextStep;
            CompletedSteps.Add(nextStep);
        }
    }
}
