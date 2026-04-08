using System.Security.Claims;

namespace HNTAS.Web.UI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the OneLogin ID from the user's claims.
        /// Checks for GovUK Simulator mode and extracts from appropriate claim.
        /// </summary>
        /// <param name="principal">The claims principal (user)</param>
        /// <param name="logger">Optional logger for debugging simulator mode</param>
        /// <returns>The OneLogin ID or null if not found</returns>
        public static string? GetOneLoginId(this ClaimsPrincipal principal, ILogger? logger = null)
        {
            if (principal == null)
                return null;

            var useGovUkSimulator = Environment.GetEnvironmentVariable("SIMULATOR_PROP4");
            var isSimulatorMode = !string.IsNullOrEmpty(useGovUkSimulator) &&
                                  useGovUkSimulator.Equals("true", StringComparison.OrdinalIgnoreCase);

            if (isSimulatorMode)
            {
                var oneLoginId = principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                logger?.LogInformation("Using GovUK Simulator. Extracted oneloginId from 'nameidentifier' claim: {OneLoginId}", oneLoginId);
                return oneLoginId;
            }

            return principal.FindFirstValue("sub");
        }
    }
}