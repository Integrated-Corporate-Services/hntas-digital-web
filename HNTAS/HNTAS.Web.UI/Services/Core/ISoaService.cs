using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface ISoaService
    {        
        Task UpdateElementSoaStatus(ElementSoaStatusUpdateRequest request);
        Task AssignAssessor(ElementSoaAssignAssessorRequest request);
        Task UpdateElementSoaStatusForExistingNetwork(ElementSoaStatusUpdateRequestForExistingNetwork request);
        Task AssignAssessorForExistingNetwork(ElementSoaAssignAssessorRequestForExistingNetwork request);
    }
}
