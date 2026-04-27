using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{    
    public class AssignedAssessorService : IAssignedAssessorService
    {
        private readonly ILogger<AssignedAssessorService> _logger;
        private readonly IAssignedAssessorApi _assignedAssessorApi;

        public AssignedAssessorService(IAssignedAssessorApi assignedAssessorApi, ILogger<AssignedAssessorService> logger)
        {
            _assignedAssessorApi = assignedAssessorApi;
            _logger = logger;
        }

        public async Task<AssignedAssessorResponse> GetAssignedAssessor(AssignedAssessorRequest request)
        {
            var response = await _assignedAssessorApi.ApiAssignedAssessorAssignedAssessorGetAsync(request);
            if (response.IsNotFound)
            {
                _logger.LogWarning("No assessors record found.");
                return new AssignedAssessorResponse();
            }

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch assessors record. Status code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to fetch assessors record. Service returned {response.StatusCode}");
            }
            return response.Ok() ?? new AssignedAssessorResponse();
        }
    }
}
