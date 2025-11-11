using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class UserService : IUserService
    {
        private readonly IUsersApi _usersApi;
        private readonly IHeatNetworksApi _heatNetworksApi;
        private readonly ILogger<UserService> _logger;

        public UserService(IUsersApi usersApi, ILogger<UserService> logger, IHeatNetworksApi heatNetworksApi)
        {
            _usersApi = usersApi;
            _logger = logger;
            _heatNetworksApi = heatNetworksApi;
        }

        public async Task<UserResponse?> GetUserById(string id)
        {
            _logger.LogInformation("Retrieving user by OneLogin ID: {OneLoginId}", id);

            try
            {
                var userResponse = await _usersApi.GetUserByIdAsync(id);

                if (userResponse.IsOk)
                {
                    return userResponse.Ok();
                }
                else if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new Exception($"Failed to retrieve user with status code: {userResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by Id: {Id}", id);
                throw;
            }
        }

        public async Task<UserResponse?> GetUserByOneLoginId(string oneLoginId)
        {
            _logger.LogInformation("Retrieving user by OneLogin ID: {OneLoginId}", oneLoginId);

            try
            {
                var userResponse = await _usersApi.GetUserByOneLoginIdAsync(oneLoginId);

                if (userResponse.IsOk)
                {
                    return userResponse.Ok();
                }
                else if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new Exception($"Failed to retrieve user with status code: {userResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by OneLogin ID: {OneLoginId}", oneLoginId);
                throw;
            }
        }

        public async Task<string?> CreateUser(InitialUserRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Creating user with OneLogin ID: {OneLoginId}", request.OneLoginId);

                var response = await _usersApi.ApiUsersInitialEntryPostAsync(request);

                if (response.IsCreated)
                {
                    // Check if the response indicates a successful creation
                    _logger.LogInformation("User created successfully with ID: {UserId}", response.Created());
                    return response.Created();
                }

                throw new Exception($"Failed to create user with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user creation for OneLogin ID: {OneLoginId}", request.OneLoginId);
                throw;
            }
        }

        public async Task<string?> UpdateUserOrganisation(string id, UpdateUserOrganisationRequest request)
        {
            _logger.LogInformation("Updating user organisation for ID: {UserId}", id);
            try
            {
                var response = await _usersApi.ApiUsersIdOrgDetailsPatchAsync(id, request);

                if (response.IsOk)
                {
                    return response.Ok()?.OrgId;
                }

                throw new Exception($"Failed to create user with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user creation for user ID: {userId}", id);
                throw;
            }
        }

        public async Task UpdateUserHeatNetworkId(string id, string heatNetworkId)
        {
            _logger.LogInformation("Updating user heat network for ID: {UserId} heat network : {hnId}", id, heatNetworkId);
            try
            {
                var response = await _usersApi.ApiUsersIdHeatnetworkHeatNetworkIdPatchAsync(id, heatNetworkId);

                if (response.IsNoContent)
                {
                    _logger.LogInformation("User heat network ID updated successfully for user ID: {userId}", id);
                    return;
                }
                throw new Exception($"Failed to update user heat network ID with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user heat network ID for user ID: {userId}", id);
                throw;
            }
        }

        public async Task<bool?> IsOrganisationExists(string companiesHouseNumber)
        {
            _logger.LogInformation("Checking if organisation with Companies House number {CompaniesHouseNumber} has RP user", companiesHouseNumber);
            try
            {
                var response = await _usersApi.ApiUsersOrganisationExistsGetAsync(companiesHouseNumber);
                if (response.IsOk)
                {
                    return response.Ok();
                }
                throw new Exception($"Failed to check organisation with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if organisation has RP user for Companies House number: {CompaniesHouseNumber}", companiesHouseNumber);
                throw;
            }
        }

        public async Task<List<HeatNetworkResponse>?> GetUserHeatNetworks(string id)
        {
            var user = await GetUserById(id);
            if (user?.HnIds == null || !user.HnIds.Any())
            {
                _logger.LogInformation("User with ID {UserId} has no heat networks assigned.", id);
                return null;
            }
            _logger.LogInformation("User with ID {UserId} has heat networks assigned: {HeatNetworkIds}", id, string.Join(", ", user.HnIds));

            try
            {
                var heatNetworkResponse = await _heatNetworksApi.ApiHeatNetworksHnIdsGetAsync(string.Join(",", user?.HnIds));
                if (heatNetworkResponse.IsOk)
                {
                    return heatNetworkResponse.Ok();
                }
                throw new Exception($"Failed to retrieve heat network with status code: {heatNetworkResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving heat networks for user ID: {UserId}", id);
                throw;
            }
        }

        public async Task<List<EnumItemResponse>> GetContributorRolesAsync()
        {
            var response = await _usersApi.ApiUsersContributorRolesGetAsync();

            if (response.IsOk)
            {
                return response.Ok();
            }
            else
            {
                _logger.LogError("Failed to retrieve contributor roles with status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to retrieve contributor roles with status code: {response.StatusCode}");
            }

        }

        public async Task<List<EnumItemResponse>> GetUserRolesAsync()
        {
            var response = await _usersApi.ApiUsersUserRolesGetAsync();

            if (response.IsOk)
            {
                return response.Ok();
            }
            else
            {
                _logger.LogError("Failed to retrieve contributor roles with status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to retrieve contributor roles with status code: {response.StatusCode}");
            }
        }


        public async Task<UserDetailsResponse> GetUserDetails(string userId)
        {

            _logger.LogInformation("Getting user details for user ID: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("User ID is null or empty");
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            try
            {
                var user = await _usersApi.ApiUsersUserDetailsByIdGetAsync(userId);
                if (user.IsOk)
                {
                    return user.Ok();
                }
                throw new Exception($"Failed to get user details with status code: {user.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ManagedUserResponse>> GetManagedUsers(string userId)
        {
            _logger.LogInformation("Getting managed users for user ID: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("User ID is null or empty");
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            try
            {
                var users = await _usersApi.ApiUsersManagedUsersGetAsync(userId);
                if (users.IsOk)
                {
                    return users.Ok();
                }
                throw new Exception($"Failed to get managed users with status code: {users.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting managed users for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<string?> AcceptUserInvitation(InvitedUserRequest userRequest)
        {
            _logger.LogInformation("Accepting user invitation for email: {Email}", userRequest.InvitedEmail);

            try
            {
                var response = await _usersApi.ApiUsersAcceptInvitationPatchOrDefaultAsync(userRequest);
                if (response.IsOk)
                {
                    _logger.LogInformation("User invitation accepted for email: {Email}", userRequest.InvitedEmail);
                    return response.Ok();
                }
                if (response.IsCreated)
                {
                    _logger.LogInformation("User invitation accepted for email: {Email}", userRequest.InvitedEmail);
                    return response.Created();
                }
                throw new Exception($"Failed to update user heat network ID with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting user invitation for email: {Email}", userRequest.InvitedEmail);
                throw;
            }

        }

        public async Task<List<UserResponse>> GetRegisteredUsersAsync(string rpUserId)
        {
            var users = await _usersApi.ApiUsersRegisteredUsersGetAsync(rpUserId);
            if (users.IsOk)
            {
                return users.Ok();
            }

            _logger.LogError("Failed to retrieve registered users with status code: {StatusCode}", users.StatusCode);
            throw new Exception($"Failed to retrieve registered users with status code: {users.StatusCode}");

        }

        public async Task<bool?> IsRpUserAsync(string emailId)
        {
            var users = await _usersApi.ApiUsersIsRpUserEmailIdGetAsync(emailId);
            if (users.IsOk)
            {
                return users.Ok();
            }
            else if (users.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            var errorMessage = $"Unable to determine Regulatory Contact status for user '{SanitizeForLogging(emailId)}'. API call failed with status code: {users.StatusCode}.";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }


        public async Task<bool?> IsActiveUserAsync(string emailId)
        {
            var users = await _usersApi.ApiUsersIsActiveUserEmailIdGetAsync(emailId);
            if (users.IsOk)
            {
                return users.Ok();
            }
            else if (users.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            var errorMessage = $"Unable to determine Regulatory Contact status for user '{SanitizeForLogging(emailId)}'. API call failed with status code: {users.StatusCode}.";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }


        private string SanitizeForLogging(string input)
        {
            return input?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
        }

    }
}


