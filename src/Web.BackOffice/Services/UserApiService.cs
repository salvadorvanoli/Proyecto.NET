using System.Net.Http.Json;
using Shared.DTOs.Users;

namespace Web.BackOffice.Services;

/// <summary>
/// Implementation of user API service using HttpClient.
/// </summary>
public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserApiService> _logger;
    private const string BaseUrl = "api/users";

    public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        try
        {
            var users = await _httpClient.GetFromJsonAsync<IEnumerable<UserResponse>>(BaseUrl);
            return users ?? Enumerable.Empty<UserResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users from API");
            throw;
        }
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} from API", id);
            throw;
        }
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest createUserDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, createUserDto);
            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<UserResponse>();
            return user ?? throw new InvalidOperationException("Failed to deserialize user response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user via API");
            throw;
        }
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest updateUserDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateUserDto);
            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<UserResponse>();
            return user ?? throw new InvalidOperationException("Failed to deserialize user response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId} via API", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId} via API", id);
            throw;
        }
    }
}
