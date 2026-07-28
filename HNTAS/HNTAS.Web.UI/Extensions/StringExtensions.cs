namespace HNTAS.Web.UI.Extensions
{
    public static class StringExtensions
    {
        public static string ToSentenceCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        public static string ToSafeLog(this string? id)
        {
            var sanitized = (id ?? string.Empty)
                .Replace("\r", "")
                .Replace("\n", "");

            var atIndex = sanitized.IndexOf('@');
            if (atIndex > 0 && atIndex < sanitized.Length - 1)
            {
                var localPart = sanitized[..atIndex];
                var domainPart = sanitized[(atIndex + 1)..];

                var maskedLocalPart = localPart.Length <= 1
                    ? "*"
                    : $"{localPart[0]}***";

                return $"{maskedLocalPart}@{domainPart}";
            }

            return sanitized;
        }

        public static string SanitizeForLogging(string input)
        {
            return input?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
        }
    }
}