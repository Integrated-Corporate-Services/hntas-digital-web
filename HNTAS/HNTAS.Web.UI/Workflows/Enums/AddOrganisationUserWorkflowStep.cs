namespace HNTAS.Web.UI.Workflows.Enums
{
    public enum AddOrganisationUserWorkflowStep
    {
        None = 0,
        AddEmailAddress = 1,
        ContactDetails = 2,
        AssignRole = 3,
        RoleAssignmentConfirmation = 4,
        ExistingUser = 5,
        CannotContinue = 6
    }
}
