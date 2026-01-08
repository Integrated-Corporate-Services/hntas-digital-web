using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Enums
{
    public enum PreferredContactType
    {
        [Display(Name = "Landline")]
        Landline = 1,
        [Display(Name = "Mobile")]
        Mobile = 2,
        [Display(Name = "Prefer not to say")]
        PreferNotToSay = 3
    }
}
