using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface ISoaService
    {
        Task<Soa2?> GetByHnIdAsync(string hnId);
        Task<Soa2?> CreateAsync(string hnId, string createdBy);
        Task UpdateNetworkTypeAsync(string hnId, string updatedBy, NetworkTypeSelection2 networkTypeSelection);
        Task UpdateConnectionsAsync(string hnId, string updatedBy, List<ConnectionType> connectionTypes);
        Task UpdateNetworkElements(string hnId, string updatedBy, List<HeatNetworkElement> networkElements);
        Task UpdateElementLocations(UpdateElementLocationsRequest request);
        Task UpdateElementDocuments(UpdateElementDocumentsRequest request);

        Task UpdateDocument(UpdateDocumentRequest request);

        Task UpdateSOAStatus(UpdateSoaStatusRequest soaStatusRequest);

        Task SendAssessorAssessmentEmail(string hnName, string hnId, string assessmentResult);

        Task SendCertificationCompleteEmail(string hnName, string hnId);
        Task UpdateElementSoaStatus(ElementSoaStatusUpdateRequest request);
        Task AssignAssessor(ElementSoaAssignAssessorRequest request);
    }
}
