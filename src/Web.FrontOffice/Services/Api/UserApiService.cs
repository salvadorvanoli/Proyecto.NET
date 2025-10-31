using System.Net.Http.Json;
using Application.Users.DTOs;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

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

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        try
        {
            // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario al hacer login
            // Por ahora se debe configurar un HttpMessageHandler que lo agregue automáticamente
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{id}");
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User {UserId} not found", id);
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserResponse>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User {UserId} not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} from API", id);
            throw;
        }
    }

    public async Task<UserResponse?> GetCurrentUserAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserResponse>($"{BaseUrl}/current");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Current user not found");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user from API");
            throw;
        }
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest updateUserRequest)
    {
        try
        {
            // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
            // Por ahora se debe configurar un HttpMessageHandler que lo agregue automáticamente
            var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/{id}")
            {
                Content = JsonContent.Create(updateUserRequest)
            };
            
            var response = await _httpClient.SendAsync(request);
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
}
