using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface ISoaProjectService
    {
        Task<SoaProject> GetAsync(string projectId);
        Task<SoaProject> GetByHnIdAsync(string hnId);
        Task<SoaProject> CreateAsync(string hnId, string createdBy);
        Task UpdateNetworkTypeAsync(string hnId, string updatedBy, NetworkTypeSelection2 networkTypeSelection);
        Task UpdateConnectionsAsync(string hnId, string updatedBy, List<ConnectionType> connectionTypes);
        Task UpdateNetworkElements(string hnId, string updatedBy, List<HeatNetworkElement> networkElements);
        Task UpdateElementLocations(UpdateElementLocationsRequest request);
        Task UpdateElementDocuments(UpdateElementDocumentsRequest request);
    }
}
