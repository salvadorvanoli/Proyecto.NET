using System.Net.Http.Json;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of tenant API service.
/// </summary>
public class TenantApiService : ITenantApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TenantApiService> _logger;

    public TenantApiService(HttpClient httpClient, ILogger<TenantApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TenantThemeDto?> GetTenantThemeAsync(int tenantId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<TenantThemeDto>($"api/tenants/{tenantId}/theme");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant theme for tenant {TenantId}", tenantId);
            return null;
        }
    }
}
