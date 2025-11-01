using Application.AccessEvents.DTOs;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of the access event API service.
/// </summary>
public class AccessEventApiService : IAccessEventApiService
{
    private readonly HttpClient _httpClient;

    public AccessEventApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId)
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync($"api/accessevents/user/{userId}");
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>();
        return events ?? new List<AccessEventResponse>();
    }

    public async Task<List<AccessEventResponse>> GetAllAccessEventsAsync()
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync("api/accessevents");
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>();
        return events ?? new List<AccessEventResponse>();
    }

    public async Task<AccessEventResponse?> GetAccessEventByIdAsync(int eventId)
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync($"api/accessevents/{eventId}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AccessEventResponse>();
    }
}
