using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IHeatNetworkService
    {
        Task<HeatNetwork> AddHeatNetwork(HeatNetwork heatNetwork, string hnId);
        Task<List<HeatNetwork>> GetAllHeatNetworks();
    }
}
