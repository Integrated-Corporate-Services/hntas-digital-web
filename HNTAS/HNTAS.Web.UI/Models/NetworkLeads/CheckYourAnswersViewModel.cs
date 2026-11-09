using HNTAS.Web.UI.ModelValidation;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class CheckYourAnswersViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? JobTitle { get; set; }

        public string? InvitedRole { get; set; }
        public bool InviteForFirstTime { get; set; }
        public bool? WorkForSameOrganisation { get; set; }
        public string? WorkingOrganisationName { get; set; }

        [MustBeTrue(ErrorMessage = "Confirm that you are authorised to proceed")]
        public bool ConfirmedDeclaration { get; set; }
    }
}
