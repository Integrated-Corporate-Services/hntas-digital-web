namespace HNTAS.Web.UI.Models.Soa
{
    public class SOAReviewSummaryViewModel
    {
        public string Phase { get; set; }
        public List<ElementItem> Elements { get; set; } = [];
        public List<DocumentItem> ElementDocuments { get; set; } = [];
        public DocumentItem AssessmentPlanDocument { get; set; }
        public DocumentItem? AssessorDocument { get; set; }
    }

    public class ElementItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class DocumentItem
    {
        public string Name { get; set; }
        public List<DocumentReference> Documents { get; set; } = new();

        public string ChangeUrl { get; set; }
    }

    public class DocumentReference
    {
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
    }
}
