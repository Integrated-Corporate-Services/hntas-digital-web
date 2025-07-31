using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class Option
    {
        [Required]
        public string OptId { get; set; }
        [Required]
        public string OptText { get; set; }
        public string? OptHintText { get; set; }
    }

    public class Question
    {
        [Required]
        public string QId { get; set; }
        [Required]
        public string QText { get; set; }
        public string? QDescText { get; set; }
        public string? QHintText { get; set; }
        [Required]
        public List<Option> Options { get; set; }
        public string? SeletedOption { get; set; }
    }

    public class QuestionWithRadiobuttonsModel
    {
        public string? PageHeading { get; set; }
        [Required]
        public List<Question> Questions { get; set; }
        [Required]
        public string Controller { get; set; }
        [Required]
        public string Action { get; set; }
        [Required]
        public string[] FieldOrder { get; set; }
    }

}
