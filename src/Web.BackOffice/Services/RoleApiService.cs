using System.Net.Http.Json;
using Shared.DTOs.Roles;

namespace Web.BackOffice.Services;

/// <summary>
/// Implementation of role API service using HttpClient.
/// </summary>
public class RoleApiService : IRoleApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RoleApiService> _logger;
    private const string BaseUrl = "api/roles";

    public RoleApiService(HttpClient httpClient, ILogger<RoleApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<RoleResponse>> GetRolesByTenantAsync()
    {
        try
        {
            var roles = await _httpClient.GetFromJsonAsync<IEnumerable<RoleResponse>>(BaseUrl);
            return roles ?? Enumerable.Empty<RoleResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles from API");
            throw;
        }
    }

    public async Task<RoleResponse?> GetRoleByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RoleResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId} from API", id);
            throw;
        }
    }

    public async Task<RoleResponse> CreateRoleAsync(RoleRequest createRoleDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, createRoleDto);
            response.EnsureSuccessStatusCode();

            var role = await response.Content.ReadFromJsonAsync<RoleResponse>();
            return role ?? throw new InvalidOperationException("Failed to deserialize role response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role via API");
            throw;
        }
    }

    public async Task<RoleResponse> UpdateRoleAsync(int id, RoleRequest updateRoleDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateRoleDto);
            response.EnsureSuccessStatusCode();

            var role = await response.Content.ReadFromJsonAsync<RoleResponse>();
            return role ?? throw new InvalidOperationException("Failed to deserialize role response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId} via API", id);
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            // If it's a bad request, the role might have users assigned
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorContent);
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId} via API", id);
            throw;
        }
    }

    public async Task AssignRolesToUserAsync(int userId, AssignRoleRequest assignRoleDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/assign/{userId}", assignRoleDto);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user {UserId} via API", userId);
            throw;
        }
    }

    public async Task<IEnumerable<RoleResponse>> GetUserRolesAsync(int userId)
    {
        try
        {
            var roles = await _httpClient.GetFromJsonAsync<IEnumerable<RoleResponse>>($"{BaseUrl}/user/{userId}");
            return roles ?? Enumerable.Empty<RoleResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId} from API", userId);
            throw;
        }
    }
}
