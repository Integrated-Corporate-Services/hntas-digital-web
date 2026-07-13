using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IImportExistingNetworksService
    {
        Task<ImportResult> ImportCsv(string stream);
    }
}
