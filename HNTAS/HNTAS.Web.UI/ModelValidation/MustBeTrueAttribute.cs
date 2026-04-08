using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.ModelValidation
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is bool boolValue && boolValue)
            {
                return ValidationResult.Success!;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}