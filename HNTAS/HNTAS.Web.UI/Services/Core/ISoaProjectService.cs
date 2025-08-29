using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface ISoaProjectService
    {
        Task<SoaProject> GetAsync(string projectId);
        Task<SoaProject> GetByHnIdAsync(string hnId);
        Task<SoaProject> CreateAsync(string hnId);
        Task UpdateNetworkTypeAsync(string hnId, NetworkTypeSelection2 networkTypeSelection);
        Task UpdateConnectionsAsync(string hnId, List<ConnectionType> connectionTypes);
    }
}
