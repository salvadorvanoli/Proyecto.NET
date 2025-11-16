using System.Net.Http.Json;
using Application.Users.DTOs;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of user registration API service.
/// </summary>
public class RegisterApiService : IRegisterApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegisterApiService> _logger;

    public RegisterApiService(HttpClient httpClient, ILogger<RegisterApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserResponse?> RegisterAsync(CreateUserRequest request)
    {
        try
        {
            // Add X-Tenant-Id header for default tenant (1)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var response = await _httpClient.PostAsJsonAsync("api/users", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("User registration failed with status code: {StatusCode}. Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();
            return userResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration API call");
            return null;
        }
    }
}
