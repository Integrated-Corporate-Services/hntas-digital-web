namespace HNTAS.Web.UI.Models.NetworkDetailsUpload
{
    public class NetworkDetailsUploadViewModel
    {
        public string? HeatNetworkName { get; set; }
        public int PhaseNumber { get; set; }
        public string? TemplateDownloadUrl { get; set; }
        public UploadedDocumentInfo? UploadedDocument { get; set; }
    }

    public class UploadedDocumentInfo
    {
        public string? FileName { get; set; }
        public DateTime UploadedDate { get; set; }
        public string? UploadedBy { get; set; }
        public string? S3Key { get; set; }
    }
}
