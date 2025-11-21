using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Enums;

namespace HNTAS.Web.UI.Extensions
{
    public static class PreferredContactTypeExtensions
    {
        public static NullableOfPreferredContactType? ToApiModelType(this PreferredContactType? preferredContactType)
        {
            return preferredContactType switch
            {
                PreferredContactType.Landline => NullableOfPreferredContactType.Landline,
                PreferredContactType.Mobile => NullableOfPreferredContactType.Mobile,
                PreferredContactType.PreferNotToSay => NullableOfPreferredContactType.PreferNotToSay,
                _ => null
            };
        }

        public static PreferredContactType? ToViewModelType(this NullableOfPreferredContactType? preferredContactType)
        {
            return preferredContactType switch
            {
                NullableOfPreferredContactType.Landline => PreferredContactType.Landline,
                NullableOfPreferredContactType.Mobile => PreferredContactType.Mobile,
                NullableOfPreferredContactType.PreferNotToSay => PreferredContactType.PreferNotToSay,
                _ => null
            };
        }

    }
}
