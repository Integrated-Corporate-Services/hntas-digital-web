namespace HNTAS.Web.UI.Helpers
{
    public static class GovUkTagHelper
    {
        private static readonly Dictionary<string, string> StatusTagMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Active",  "govuk-tag--green"},
            { "Invited", "govuk-tag--blue" },
            { "Accepted", "govuk-tag--green" },
            { "Rejected", "govuk-tag--orange" },
            { "Not yet started", "govuk-tag--blue" },
            { "In Progress", "govuk-tag--blue" },
            { "Completed",  "govuk-tag--green"}
        };

        public static string GetTagClass(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "govuk-tag--grey";

            return StatusTagMap.TryGetValue(status.Trim(), out var cssClass)
                ? cssClass
                : "govuk-tag--grey"; // fallback
        }
    }
}
