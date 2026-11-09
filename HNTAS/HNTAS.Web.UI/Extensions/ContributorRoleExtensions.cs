using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Extensions
{
    public static class ContributorRoleExtensions
    {
        public static string GetDisplayName(this ContributorRole role) => role switch
        {
            ContributorRole.DesignatedDutyHolder => "Designated duty holder",
            ContributorRole.Contributor => "Contributor",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
}
