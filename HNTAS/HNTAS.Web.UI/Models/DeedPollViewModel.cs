using HNTAS.Web.UI.ModelValidation;

namespace HNTAS.Web.UI.Models
{
    public class DeedPollViewModel
    {
        [MustBeTrue(ErrorMessage = "Confirm your organisation's agreement to the HNTAS terms and conditions")]
        public bool IsDeedPollAccepted { get; set; }
    }
}
