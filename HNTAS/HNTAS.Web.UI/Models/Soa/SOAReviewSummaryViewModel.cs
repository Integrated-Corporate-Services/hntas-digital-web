namespace HNTAS.Web.UI.Models.Soa
{
    public class SOAReviewSummaryViewModel
    {
        public string Phase { get; set; }
        public List<ElementItem> Elements { get; set; } = [];
        public List<DocumentItem> ElementDocuments { get; set; } = [];
        public DocumentItem AssessmentPlanDocument { get; set; }
    }

    public class ElementItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class DocumentItem
    {
        public string Name { get; set; }
        public List<string> DocNames { get; set; } = [];
        public string ChangeUrl { get; set; }
    }
}
