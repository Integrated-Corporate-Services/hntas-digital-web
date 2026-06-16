namespace HNTAS.Web.UI.Services
{
    public interface IS3UploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string keyPrefix);
        Task<Stream?> GetFileAsync(string key);
    }
}
