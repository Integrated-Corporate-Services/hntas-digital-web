using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IEnergyCentrePlantDataService
    {
        Task<List<FuelType>?> GetFuelTypes();
        Task<List<CHPOperationalMode>?> GetChpOperationalModes();
        Task<List<HeatRecovery>?> GetHeatRecoverySources();
    }
}
