using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class InvitationService : IInvitationService
    {
        private readonly ILogger<InvitationService> _logger;
        private readonly IInvitationsApi _invitationsApi;
        public InvitationService(ILogger<InvitationService> logger, IInvitationsApi invitationsApi)
        {
            _logger = logger;
            _invitationsApi = invitationsApi;
        }

        public async Task<InvitedUserResponse?> GetInvitationByIdAsync(string id)
        {
            _logger.LogInformation("Fetching invitation with ID: {InvitationId}", id);
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("Invitation ID is null or empty");
                throw new ArgumentNullException(nameof(id), "Invitation ID cannot be null or empty");
            }
            try
            {
                var response = await _invitationsApi.ApiInvitationsIdGetAsync(id);
                if (response.IsOk)
                {
                    _logger.LogInformation("Invitation with ID: {InvitationId} fetched successfully", id);
                    return response.Ok();
                }
                else if (response.IsNotFound)
                {
                    _logger.LogWarning("Invitation with ID: {InvitationId} not found", id);
                    return null;
                }
                throw new Exception($"Failed to fetch invitation with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invitation with ID: {InvitationId}", id);
                throw;
            }
        }

        public async Task<string?> AddInvitedUserAsync(string id, AddInvitationRequest invitationRequest)
        {
            _logger.LogInformation("Updating invited user with ID: {UserId}", id);
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("User ID is null or empty");
                throw new ArgumentNullException(nameof(id), "User ID cannot be null or empty");
            }
            if (invitationRequest == null)
            {
                _logger.LogError("Invitation request is null for user ID: {UserId}", id);
                throw new ArgumentNullException(nameof(invitationRequest), "Invitation request cannot be null");
            }

            try
            {
                var response = await _invitationsApi.ApiInvitationsIdAddUserInvitationPostAsync(id, invitationRequest);

                if (response.IsCreated)
                {
                    _logger.LogInformation("Invited user with ID: {UserId} updated successfully", id);
                    return response.Created();
                }
                throw new Exception($"Failed to update Invited user with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invited user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task SendInvitationEmailAsync(string invitationId, SendInvitationEmailRequest request)
        {
            _logger.LogInformation("Sending invitation email for invitation ID: {InvitationId}", invitationId);
            try
            {
                var response = await _invitationsApi.ApiInvitationsInvitationIdSendEmailPostAsync(invitationId, request);
                if (response.IsNoContent)
                {
                    _logger.LogInformation("Invitation email sent successfully for invitation ID: {InvitationId}", invitationId);
                    return;
                }
                else
                {
                    throw new Exception($"Failed to send invitation email with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invitation email for invitation ID: {InvitationId}", invitationId);
                throw;
            }
        }

        public async Task<string?> AcceptInvitationAsync(InvitedUserRequest invitationRequest)
        {
            _logger.LogInformation("Accepting invitation with ID: {InvitationId}", invitationRequest.InvitationId);
            try
            {
                var response = await _invitationsApi.ApiInvitationsAcceptInvitationPatchAsync(invitationRequest);

                if (response.IsCreated)
                    return response.Created();

                if (response.IsOk)
                    return response.Ok();

                if (response.IsNotFound)
                {
                    _logger.LogWarning("Invitation not found: {InvitationId}", invitationRequest.InvitationId);
                    return null;
                }

                if (response.IsConflict)
                {
                    throw new Exception("Conflict occurred while accepting invitation");
                }

                throw new Exception($"Unexpected status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while accepting invitation ID: {InvitationId}", invitationRequest.InvitationId);
                throw;
            }
        }

        public async Task RejectInvitationAsync(string invitationId)
        {
            try
            {
                var response = await _invitationsApi.ApiInvitationsInvitationIdRejectPostOrDefaultAsync(invitationId);
                if (response != null && response.IsNoContent)
                {
                    _logger.LogInformation("Successfully rejected invitation ID: {InvitationId}", invitationId);
                    return;
                }
                throw new Exception($"Failed to send invitation email with status code: {response?.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while rejecting invitation ID: {InvitationId}", invitationId);
                throw;
            }
        }
    }
}
