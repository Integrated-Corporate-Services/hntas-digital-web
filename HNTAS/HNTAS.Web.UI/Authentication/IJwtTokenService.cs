namespace HNTAS.Web.UI.Authentication
{
    public interface IJwtTokenService
    {
        string GenerateToken(string oneLoginId);
    }
}
