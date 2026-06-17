using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IAssignedAssessorService
    {
        Task<AssignedAssessorResponse> GetAssignedAssessor(AssignedAssessorRequest request);
    }
}
