namespace HNTAS.Web.UI.Models
{
    public class PdfRow { 
        public string LinkToPdf { get; set; }
        public string PdfTitle { get; set; }
        public string UpdatedBy { get; set; } = "October 2025";
        public int SizeInKb { get; set; }
        public int NumberOfPages { get; set; }
    }

    public class DocumentListSection
    {
        public string SectionName { get; set; }
        public string SectionId { get; set; }
        public List<PdfRow> PdfRows { get; set; }
    }
    public class HntasElementDocumentsLinksViewModel
    {
        public string ElementName { get; set; }
        public string Description { get; set; }
        public List<DocumentListSection> DocumentListSections { get; set; }
    }
}
