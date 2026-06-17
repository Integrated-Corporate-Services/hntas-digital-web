using Microsoft.AspNetCore.DataProtection;

namespace HNTAS.Web.UI.Services
{
    public class InvitationTokenService : IInvitationTokenService
    {
        private readonly IDataProtector _protector;

        public InvitationTokenService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("InvitationEmailToken");
        }

        public string GenerateToken(string invitationId, string invitationEmail)
        {
            var payload = $"{invitationId}|{invitationEmail}|{DateTime.UtcNow:O}";
            return _protector.Protect(payload);
        }

        public (string? InvitationId, string? Email) DecryptToken(string token)
        {
            try
            {
                var unprotected = _protector.Unprotect(token);
                var parts = unprotected.Split('|');
                if (parts.Length < 2) return (null, null);

                return (parts[0], parts[1]);
            }
            catch
            {
                return (null, null); // Token is invalid or tampered
            }
        }
    }
}
