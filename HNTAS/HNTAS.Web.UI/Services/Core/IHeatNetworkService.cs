using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IHeatNetworkService
    {
        Task<HeatNetworkResponse?> GetAsync(string hnId);
        Task<HeatNetworkResponse> AddHeatNetwork(HeatNetwork heatNetwork, string hnId);
        Task<List<HeatNetworkResponse>> GetAllHeatNetworks();
        Task<bool?> GetAssessorImpartialityAsync(string hnId);
        Task<bool> SetAssessorImpartialityAsync(string hnId);
    }
}
