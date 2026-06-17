using HNTAS.Web.UI.Models.Components;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.CustomValidation
{
    public class MustHaveOneItemAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }

            return false;
        }
    }

    public class RequiredIfSelectedAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var instance = context.ObjectInstance as CheckboxItemWithConditionalInput;

            if (instance != null && instance.IsSelected)
            {
                if (value == null)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }

    public class MustHaveOneHnConnectionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is IEnumerable<CheckboxItemWithConditionalInput> items)
            {
                if (items.Any(x => x.IsSelected))
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(ErrorMessage ?? "Select at least one option");
            }

            return new ValidationResult("Invalid data");
        }
    }
}
