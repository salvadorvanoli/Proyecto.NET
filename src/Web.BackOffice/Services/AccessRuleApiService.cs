using System.Net.Http.Json;
using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for access rule API operations.
/// </summary>
public class AccessRuleApiService : IAccessRuleApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/accessrules";

    public AccessRuleApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AccessRuleDto?> GetAccessRuleByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AccessRuleDto>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<AccessRuleDto>> GetAccessRulesByTenantAsync()
    {
        try
        {
            var accessRules = await _httpClient.GetFromJsonAsync<IEnumerable<AccessRuleDto>>(BaseUrl);
            return accessRules ?? Enumerable.Empty<AccessRuleDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<AccessRuleDto>();
        }
    }

    public async Task<IEnumerable<AccessRuleDto>> GetAccessRulesByControlPointAsync(int controlPointId)
    {
        try
        {
            var accessRules = await _httpClient.GetFromJsonAsync<IEnumerable<AccessRuleDto>>($"{BaseUrl}/controlpoint/{controlPointId}");
            return accessRules ?? Enumerable.Empty<AccessRuleDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<AccessRuleDto>();
        }
    }

    public async Task<IEnumerable<AccessRuleDto>> GetAccessRulesByRoleAsync(int roleId)
    {
        try
        {
            var accessRules = await _httpClient.GetFromJsonAsync<IEnumerable<AccessRuleDto>>($"{BaseUrl}/role/{roleId}");
            return accessRules ?? Enumerable.Empty<AccessRuleDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<AccessRuleDto>();
        }
    }

    public async Task<AccessRuleDto?> CreateAccessRuleAsync(CreateAccessRuleDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AccessRuleDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateAccessRuleAsync(int id, UpdateAccessRuleDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAccessRuleAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
