namespace HNTAS.Web.UI.Models.Components
{
    public class GovUkInputModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string? Hint { get; set; }
        public bool? IsDisabled { get; set; }
        public bool? IsReadOnly { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AdditionalClasses { get; set; }
    }
}
