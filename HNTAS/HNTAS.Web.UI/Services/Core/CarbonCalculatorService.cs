using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
namespace HNTAS.Web.UI.Services.Core
{
    public class CarbonCalculatorService : ICarbonCalculatorService
    {
        private readonly ICarbonCalculatorApi _carbonCalculatorApi;
        private readonly ILogger<CarbonCalculatorService> _logger;
        public CarbonCalculatorService(ICarbonCalculatorApi carbonCalculatorApi, ILogger<CarbonCalculatorService> logger)
        {
            _carbonCalculatorApi = carbonCalculatorApi;
            _logger = logger;
        }

        public async Task<IApiCarbonCalculatorRunPostApiResponse?> CalculateAsync(CarbonCalculatorRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _carbonCalculatorApi.ApiCarbonCalculatorRunPostAsync(request, cancellationToken: cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating carbon emissions");
                return null;
            }
        }
    }
}
