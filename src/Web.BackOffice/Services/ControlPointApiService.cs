using System.Net.Http.Json;
using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for control point API operations.
/// </summary>
public class ControlPointApiService : IControlPointApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/controlpoints";

    public ControlPointApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ControlPointDto?> GetControlPointByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ControlPointDto>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<ControlPointDto>> GetControlPointsBySpaceAsync(int spaceId)
    {
        try
        {
            var controlPoints = await _httpClient.GetFromJsonAsync<IEnumerable<ControlPointDto>>($"{BaseUrl}/space/{spaceId}");
            return controlPoints ?? Enumerable.Empty<ControlPointDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<ControlPointDto>();
        }
    }

    public async Task<IEnumerable<ControlPointDto>> GetControlPointsByTenantAsync()
    {
        try
        {
            var controlPoints = await _httpClient.GetFromJsonAsync<IEnumerable<ControlPointDto>>(BaseUrl);
            return controlPoints ?? Enumerable.Empty<ControlPointDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<ControlPointDto>();
        }
    }

    public async Task<ControlPointDto?> CreateControlPointAsync(CreateControlPointDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ControlPointDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateControlPointAsync(int id, UpdateControlPointDto dto)
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

    public async Task<bool> DeleteControlPointAsync(int id)
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
