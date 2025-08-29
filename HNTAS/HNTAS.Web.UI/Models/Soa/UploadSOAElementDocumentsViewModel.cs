namespace HNTAS.Web.UI.Models.Soa
{
    public class UploadSOAElementDocumentsViewModel
    {
        public string ProjectName { get; set; } = "Olympic Park Aberdeen";
        public string PageTitle { get; set; } = "Upload SOA Documents";
        public string ElementName { get; set; }
        public string ElementDescription { get; set; }
        public List<DocumentUploadModel> Documents { get; set; }
    }
    public class DocumentUploadModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string FileInputId { get; set; }
    }
}
