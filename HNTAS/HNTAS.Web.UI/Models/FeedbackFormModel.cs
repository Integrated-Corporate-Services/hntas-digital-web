using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class FeedbackFormModel
    {
        [Required(ErrorMessage = "Select an option")]
        public string SatisfactionLevel { get; set; }
        public string? Feedback { get; set; }
    }
}
