using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using System.Diagnostics.CodeAnalysis;

namespace HNTAS.Web.UI.Services.Core
{
    [ExcludeFromCodeCoverage]
    public class ImportExistingNetworksService : IImportExistingNetworksService
    {
        private readonly IImportApi _importApi;
        private readonly ILogger<NotificationHistoryService> _logger;

        public ImportExistingNetworksService(IImportApi importApi, ILogger<NotificationHistoryService> logger)
        {
            _importApi = importApi;
            _logger = logger;
        }

        public async Task<ImportResult> ImportCsv(string stream)
        {
            try
            {                
                var response = await _importApi.ApiImportUploadCsvPostAsync(stream);

                if (!response.IsOk)
                {
                    _logger.LogError("Failed to process CSV record(s). Status code: {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"Failed to process CSV record(s). Service returned {response.StatusCode}");
                }

                return response.Ok() ?? new ImportResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while importing CSV file.");
                throw;
            }
        }
    }
}
