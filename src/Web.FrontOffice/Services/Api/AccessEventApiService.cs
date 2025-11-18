using System.Net.Http.Json;
using Application.AccessEvents.DTOs;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of the access event API service.
/// </summary>
public class AccessEventApiService : IAccessEventApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccessEventApiService> _logger;

    public AccessEventApiService(HttpClient httpClient, ILogger<AccessEventApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId)
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/access-events/user/{userId}");
        // TESTING: Agregar header X-Tenant-Id hardcodeado
        request.Headers.Add("X-Tenant-Id", "1"); // ⚠️ CAMBIAR: Usar TenantId del usuario que quieres probar
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>();
        return events ?? new List<AccessEventResponse>();
    }

    public async Task<List<AccessEventResponse>> GetAllAccessEventsAsync()
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync("api/access-events");
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>();
        return events ?? new List<AccessEventResponse>();
    }

    public async Task<AccessEventResponse?> GetAccessEventByIdAsync(int eventId)
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync($"api/access-events/{eventId}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AccessEventResponse>();
    }

    public async Task<AccessEventResponse> CreateAccessEventAsync(CreateAccessEventRequest request)
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/access-events")
        {
            Content = JsonContent.Create(request)
        };
        // TESTING: Agregar header X-Tenant-Id hardcodeado
        httpRequest.Headers.Add("X-Tenant-Id", "1"); // ⚠️ CAMBIAR: Usar TenantId del usuario que quieres probar
        
        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var accessEvent = await response.Content.ReadFromJsonAsync<AccessEventResponse>();
        return accessEvent ?? throw new InvalidOperationException("Failed to deserialize access event response");
    }
}
