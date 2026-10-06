namespace HNTAS.Web.UI.Authentication
{
    public interface IApiTokenProvider
    {
        Task<string?> GetTokenAsync();
    }
}
