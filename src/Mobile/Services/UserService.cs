using Mobile.Models;
using System.Net.Http.Json;

namespace Mobile.Services;

// Modelo interno para deserializar la respuesta del backend
internal class UserResponseBackend
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserService : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;

    public UserService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    public async Task<UserProfileDto?> GetProfileAsync()
    {
        try
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("UserService: No current user");
                return null;
            }

            var httpClient = _httpClientFactory.CreateClient("UserClient");
            
            // Agregar token de autorización
            if (!string.IsNullOrEmpty(currentUser.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentUser.Token);
            }
            
            System.Diagnostics.Debug.WriteLine($"UserService: Fetching profile for user {currentUser.UserId}");
            var response = await httpClient.GetAsync($"/api/users/{currentUser.UserId}");
            
            System.Diagnostics.Debug.WriteLine($"UserService: Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userResponse = System.Text.Json.JsonSerializer.Deserialize<UserResponseBackend>(json, new System.Text.Json.JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                if (userResponse != null)
                {
                    var profile = new UserProfileDto
                    {
                        Id = userResponse.Id,
                        Email = userResponse.Email ?? "",
                        FirstName = userResponse.FirstName ?? "",
                        LastName = userResponse.LastName ?? "",
                        BirthDate = userResponse.DateOfBirth == default ? null : userResponse.DateOfBirth,
                        IsActive = true, // Si el servidor respondió OK, el usuario está activo
                        CreatedAt = userResponse.CreatedAt
                    };
                    System.Diagnostics.Debug.WriteLine($"UserService: Profile loaded successfully");
                    return profile;
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"UserService: Error response: {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UserService: GetProfile exception: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    public async Task<bool> IsUserActiveAsync()
    {
        // Solo validar si hay conectividad
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            return true; // Modo offline - mantener sesión
        }

        try
        {
            var profile = await GetProfileAsync();
            if (profile == null)
            {
                // Si no se pudo obtener el perfil, mantener sesión
                return true;
            }
            return profile.IsActive;
        }
        catch
        {
            // Si hay error de red, mantener sesión (beneficio de la duda)
            return true;
        }
    }
}
