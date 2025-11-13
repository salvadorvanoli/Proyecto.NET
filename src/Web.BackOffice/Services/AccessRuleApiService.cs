using System.Net.Http.Json;
using Shared.DTOs.AccessRules;

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

    public async Task<AccessRuleResponse?> GetAccessRuleByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AccessRuleResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByTenantAsync()
    {
        try
        {
            var accessRules = await _httpClient.GetFromJsonAsync<IEnumerable<AccessRuleResponse>>(BaseUrl);
            return accessRules ?? Enumerable.Empty<AccessRuleResponse>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<AccessRuleResponse>();
        }
    }

    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByControlPointAsync(int controlPointId)
    {
        try
        {
            var accessRules = await _httpClient.GetFromJsonAsync<IEnumerable<AccessRuleResponse>>($"{BaseUrl}/controlpoint/{controlPointId}");
            return accessRules ?? Enumerable.Empty<AccessRuleResponse>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<AccessRuleResponse>();
        }
    }

    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByRoleAsync(int roleId)
    {
        try
        {
            var accessRules = await _httpClient.GetFromJsonAsync<IEnumerable<AccessRuleResponse>>($"{BaseUrl}/role/{roleId}");
            return accessRules ?? Enumerable.Empty<AccessRuleResponse>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<AccessRuleResponse>();
        }
    }

    public async Task<AccessRuleResponse?> CreateAccessRuleAsync(AccessRuleRequest dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AccessRuleResponse>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateAccessRuleAsync(int id, AccessRuleRequest dto)
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
