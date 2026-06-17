using HNTAS.Web.UI.Models.OrganisationRole;
using HNTAS.Web.UI.Workflows.Enums;

namespace HNTAS.Web.UI.Workflows.Models.Data
{
    public class AddExistingOrganisationUserWorkflowModel : IWorkflowModel<ExistingOrganisationUserWorkflowStep>
    {
        public ChooseUserModel? ChooseUserModel { get; set; }

        public RoleAssignmentModel? RoleAssignmentModel { get; set; }
        public HashSet<ExistingOrganisationUserWorkflowStep> CompletedSteps { get; set; } = new();
        public ExistingOrganisationUserWorkflowStep CurrentStep { get; set; }

        public void AdvanceToStep(ExistingOrganisationUserWorkflowStep nextStep)
        {
            CurrentStep = nextStep;
            CompletedSteps.Add(nextStep);
        }
    }
}
