using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.NetworkElements;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IHeatNetworkService
    {
        Task<HeatNetworkResponse?> GetAsync(string hnId);
        Task<HeatNetworkResponse> AddHeatNetwork(HeatNetwork heatNetwork);
        //Task<HeatNetworkResponse> UpdateNetworkCharacteristics(string hnId, NetworkCharacteristics2 request);
        Task<List<HeatNetworkResponse>> GetAllHeatNetworks();
        Task<HeatNetworkResponse> UpdateNetworkElements(string hnId, NetworkElements2 request);
        Task UpdateDocument(NetworkDetailsUploadDocumentRequest request);
        Task<List<HeatNetworkResponse>> GetHeatNetworkByUserId(string userId);
    }
}
