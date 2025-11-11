using System.Net.Http.Json;
using Shared.DTOs.SpaceTypes;

namespace Web.BackOffice.Services;

/// <summary>
/// Implementation of space type API service using HttpClient.
/// </summary>
public class SpaceTypeApiService : ISpaceTypeApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpaceTypeApiService> _logger;
    private const string BaseUrl = "api/spacetypes";

    public SpaceTypeApiService(HttpClient httpClient, ILogger<SpaceTypeApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<SpaceTypeResponse>> GetSpaceTypesByTenantAsync()
    {
        try
        {
            var spaceTypes = await _httpClient.GetFromJsonAsync<IEnumerable<SpaceTypeResponse>>(BaseUrl);
            return spaceTypes ?? Enumerable.Empty<SpaceTypeResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving space types from API");
            throw;
        }
    }

    public async Task<SpaceTypeResponse?> GetSpaceTypeByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SpaceTypeResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving space type {SpaceTypeId} from API", id);
            throw;
        }
    }

    public async Task<SpaceTypeResponse> CreateSpaceTypeAsync(SpaceTypeRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
            response.EnsureSuccessStatusCode();

            var spaceType = await response.Content.ReadFromJsonAsync<SpaceTypeResponse>();
            return spaceType ?? throw new InvalidOperationException("Failed to deserialize space type response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating space type via API");
            throw;
        }
    }

    public async Task<SpaceTypeResponse> UpdateSpaceTypeAsync(int id, SpaceTypeRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request);
            response.EnsureSuccessStatusCode();

            var spaceType = await response.Content.ReadFromJsonAsync<SpaceTypeResponse>();
            return spaceType ?? throw new InvalidOperationException("Failed to deserialize space type response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating space type {SpaceTypeId} via API", id);
            throw;
        }
    }

    public async Task<bool> DeleteSpaceTypeAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            // If it's a bad request, the space type might have spaces assigned
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorContent);
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting space type {SpaceTypeId} via API", id);
            throw;
        }
    }
}
