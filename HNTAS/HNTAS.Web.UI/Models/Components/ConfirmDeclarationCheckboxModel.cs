using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Components
{
    public class ConfirmDeclarationCheckboxModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public bool Value { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AdditionalClasses { get; set; }
    }    
}
