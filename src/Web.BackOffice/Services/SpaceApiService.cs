using System.Net.Http.Json;
using Shared.DTOs.Spaces;

namespace Web.BackOffice.Services;

/// <summary>
/// Implementation of space API service using HttpClient.
/// </summary>
public class SpaceApiService : ISpaceApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpaceApiService> _logger;
    private const string BaseUrl = "api/spaces";

    public SpaceApiService(HttpClient httpClient, ILogger<SpaceApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<SpaceResponse>> GetSpacesByTenantAsync()
    {
        try
        {
            var spaces = await _httpClient.GetFromJsonAsync<IEnumerable<SpaceResponse>>(BaseUrl);
            return spaces ?? Enumerable.Empty<SpaceResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving spaces from API");
            throw;
        }
    }

    public async Task<SpaceResponse?> GetSpaceByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SpaceResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving space {SpaceId} from API", id);
            throw;
        }
    }

    public async Task<SpaceResponse> CreateSpaceAsync(SpaceRequest createSpaceDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, createSpaceDto);
            response.EnsureSuccessStatusCode();

            var space = await response.Content.ReadFromJsonAsync<SpaceResponse>();
            return space ?? throw new InvalidOperationException("Failed to deserialize space response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating space via API");
            throw;
        }
    }

    public async Task<SpaceResponse> UpdateSpaceAsync(int id, SpaceRequest updateSpaceDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateSpaceDto);
            response.EnsureSuccessStatusCode();

            var space = await response.Content.ReadFromJsonAsync<SpaceResponse>();
            return space ?? throw new InvalidOperationException("Failed to deserialize space response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating space {SpaceId} via API", id);
            throw;
        }
    }

    public async Task<bool> DeleteSpaceAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            // If it's a bad request, the space might have control points assigned
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
            _logger.LogError(ex, "Error deleting space {SpaceId} via API", id);
            throw;
        }
    }
}
