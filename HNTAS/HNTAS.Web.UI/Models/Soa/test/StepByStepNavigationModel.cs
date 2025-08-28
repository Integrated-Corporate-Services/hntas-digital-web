namespace HNTAS.Web.UI.Models.Soa.test
{
    public class StepByStepGuideModel
    {
        public string Title { get; set; } = "Define your SOA";
        public List<StepDetail> Steps { get; set; } = [];
    }

    public class StepDetail
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool IsCurrent { get; set; }
        public bool IsExpanded { get; set; }
    }
}
