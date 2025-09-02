using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IHeatNetworkService
    {
        Task<HeatNetwork?> GetAsync(string hnId);
        Task<HeatNetwork> AddHeatNetwork(HeatNetwork heatNetwork, string hnId);
    }
}
