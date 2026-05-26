using HNTAS.Web.UI.ModelValidation;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class CheckYourAnswersViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? HeatNetwork { get; set; }
        public string? RoleAssigned { get; set; }
        public List<string>? SelectedPhases { get; set; }
        [MustBeTrue(ErrorMessage = "Confirm that you are authorised to proceed.")]
        public bool ConfirmedDeclaration { get; set; }
    }
}
