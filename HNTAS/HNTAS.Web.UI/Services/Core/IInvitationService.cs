using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IInvitationService
    {
        Task<InvitedUserResponse?> GetInvitationByIdAsync(string id);
        Task<string?> AddInvitedUserAsync(string id, AddInvitationRequest invitationRequest);

        Task SendInvitationEmailAsync(string invitationId, SendInvitationEmailRequest request);
    }
}
