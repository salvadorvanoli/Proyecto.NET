using System.Net.Http.Json;
using Web.BackOffice.Models;

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

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>(BaseUrl);
            return users ?? Enumerable.Empty<UserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users from API");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"{BaseUrl}/{id}");
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

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, createUserDto);
            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user ?? throw new InvalidOperationException("Failed to deserialize user response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user via API");
            throw;
        }
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateUserDto);
            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
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
