using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HNTAS.Web.UI.Models.Components
{
    public class CheckboxItemWithConditionalInput
    {
        public string? Label { get; set; }
        public string? Value { get; set; }
        public string? HintText { get; set; }
        public bool IsSelected { get; set; }
        public string? ConditionalLabel { get; set; }
        public virtual object? ConditionalValue { get; set; }
    }
}
