namespace HNTAS.Web.UI.Models.Soa
{
    public class StepByStepGuideModel
    {
        //public string Title { get; set; } = "Define your SOA";
        public List<StepNavItem> Steps { get; set; } = [];
    }

    public class StepNavItem
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = null!;
        public string BodyContent { get; set; } = null!;
        public string LinkText { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool IsCurrent { get; set; }
        public bool IsExpanded { get; set; }
    }
}
