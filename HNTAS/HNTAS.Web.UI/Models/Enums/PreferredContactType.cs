using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Enums
{
    public enum PreferredContactType
    {
        [Display(Name = "Landline")]
        Landline = 0,
        [Display(Name = "Mobile")]
        Mobile = 1
    }
}
