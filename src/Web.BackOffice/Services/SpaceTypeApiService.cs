using System.Net.Http.Json;
using Web.BackOffice.Models;

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

    public async Task<IEnumerable<SpaceTypeDto>> GetAllSpaceTypesAsync()
    {
        try
        {
            var spaceTypes = await _httpClient.GetFromJsonAsync<IEnumerable<SpaceTypeDto>>(BaseUrl);
            return spaceTypes ?? Enumerable.Empty<SpaceTypeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving space types from API");
            throw;
        }
    }

    public async Task<SpaceTypeDto?> GetSpaceTypeByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SpaceTypeDto>($"{BaseUrl}/{id}");
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

    public async Task<SpaceTypeDto> CreateSpaceTypeAsync(CreateSpaceTypeDto createSpaceTypeDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, createSpaceTypeDto);
            response.EnsureSuccessStatusCode();

            var spaceType = await response.Content.ReadFromJsonAsync<SpaceTypeDto>();
            return spaceType ?? throw new InvalidOperationException("Failed to deserialize space type response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating space type via API");
            throw;
        }
    }

    public async Task<SpaceTypeDto> UpdateSpaceTypeAsync(int id, UpdateSpaceTypeDto updateSpaceTypeDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateSpaceTypeDto);
            response.EnsureSuccessStatusCode();

            var spaceType = await response.Content.ReadFromJsonAsync<SpaceTypeDto>();
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
