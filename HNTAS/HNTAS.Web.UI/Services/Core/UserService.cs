using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class UserService : IUserService
    {
        private readonly IUsersApi _usersApi;
        private readonly ILogger<UserService> _logger;

        public UserService(IUsersApi usersApi, ILogger<UserService> logger)
        {
            _usersApi = usersApi;
            _logger = logger;
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

                if(userResponse.IsOk)
                {
                    return userResponse.Ok();
                }
                else if(userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
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

        public async Task<string?> UpdateUserOrganisation(string id, UpdateOrgDetailsAndRolesRequest request)
        {
            _logger.LogInformation("Updating user organisation for ID: {UserId}", id);
            try
            {
                var response = await _usersApi.ApiUsersIdOrgDetailsPatchAsync(id, request);

                if (response.IsOk)
                {
                    return response.Ok()?.OrgDetails?.OrgId;
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
               var responce = await _usersApi.ApiUsersIdHeatnetworkHeatNetworkIdPatchAsync(id, heatNetworkId);

                if (responce.IsNoContent)
                {
                    _logger.LogInformation("User heat network ID updated successfully for user ID: {userId}", id);
                    return;
                }
                throw new Exception($"Failed to update user heat network ID with status code: {responce.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user heat network ID for user ID: {userId}", id);
                throw;
            }
        }

        public async Task<bool?> IsOrganisationHasRpUser(string companiesHouseNumber)
        {
           _logger.LogInformation("Checking if organisation with Companies House number {CompaniesHouseNumber} has RP user", companiesHouseNumber);
            try
            {
                var response = await _usersApi.ApiUsersOrganisationExistsCompaniesHouseNumberGetAsync(companiesHouseNumber);
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
    }
}
