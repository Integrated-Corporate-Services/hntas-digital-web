using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface ICarbonCalculatorService
    {       
        Task<IApiCarbonCalculatorRunPostApiResponse?> CalculateAsync(CarbonCalculatorRequest request, CancellationToken cancellationToken = default);
    }
}
