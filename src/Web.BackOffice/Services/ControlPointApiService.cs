using System.Net.Http.Json;
using Shared.DTOs.ControlPoints;

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

    public async Task<ControlPointResponse?> GetControlPointByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ControlPointResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<ControlPointResponse>> GetControlPointsBySpaceAsync(int spaceId)
    {
        try
        {
            var controlPoints = await _httpClient.GetFromJsonAsync<IEnumerable<ControlPointResponse>>($"{BaseUrl}/space/{spaceId}");
            return controlPoints ?? Enumerable.Empty<ControlPointResponse>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<ControlPointResponse>();
        }
    }

    public async Task<IEnumerable<ControlPointResponse>> GetControlPointsByTenantAsync()
    {
        try
        {
            var controlPoints = await _httpClient.GetFromJsonAsync<IEnumerable<ControlPointResponse>>(BaseUrl);
            return controlPoints ?? Enumerable.Empty<ControlPointResponse>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<ControlPointResponse>();
        }
    }

    public async Task<ControlPointResponse?> CreateControlPointAsync(ControlPointRequest dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ControlPointResponse>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateControlPointAsync(int id, ControlPointRequest dto)
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
