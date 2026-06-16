namespace HNTAS.Web.UI.Services
{
    public interface IInvitationTokenService
    {
        string GenerateToken(string invitationId, string invitationEmail);
        (string? InvitationId, string? Email) DecryptToken(string token);
    }
}
