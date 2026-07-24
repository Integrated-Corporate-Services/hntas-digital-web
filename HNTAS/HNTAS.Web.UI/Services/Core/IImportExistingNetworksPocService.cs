using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IImportExistingNetworksPocService
    {
        Task<ImportResult> ImportCsv(string stream);
    }
}
