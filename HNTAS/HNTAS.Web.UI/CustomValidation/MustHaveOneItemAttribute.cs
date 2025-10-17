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
}
