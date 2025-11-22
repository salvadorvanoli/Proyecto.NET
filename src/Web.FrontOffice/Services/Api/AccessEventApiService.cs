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
        var response = await _httpClient.GetAsync($"api/access-events/user/{userId}");
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>();
        return events ?? new List<AccessEventResponse>();
    }

    public async Task<(List<AccessEventResponse> Events, int TotalCount)> GetUserAccessEventsPagedAsync(
        int userId, 
        int skip = 0, 
        int take = 20)
    {
        try
        {
            // Use the my-events endpoint which supports pagination
            var response = await _httpClient.GetAsync($"api/access-events/my-events?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();

            var events = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>();
            var eventsList = events ?? new List<AccessEventResponse>();

            // Get total count
            var countResponse = await _httpClient.GetAsync("api/access-events/my-events/count");
            countResponse.EnsureSuccessStatusCode();
            var totalCount = await countResponse.Content.ReadFromJsonAsync<int>();

            _logger.LogInformation(
                "Retrieved {Count} access events (total: {Total}, skip: {Skip}, take: {Take})", 
                eventsList.Count, totalCount, skip, take);

            return (eventsList, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated access events for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<AccessEventResponse>> GetAllAccessEventsAsync()
    {
        var response = await _httpClient.GetAsync("api/access-events");
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>();
        return events ?? new List<AccessEventResponse>();
    }

    public async Task<AccessEventResponse?> GetAccessEventByIdAsync(int eventId)
    {
        var response = await _httpClient.GetAsync($"api/access-events/{eventId}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AccessEventResponse>();
    }

    public async Task<AccessEventResponse> CreateAccessEventAsync(CreateAccessEventRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/access-events", request);
        response.EnsureSuccessStatusCode();

        var accessEvent = await response.Content.ReadFromJsonAsync<AccessEventResponse>();
        return accessEvent ?? throw new InvalidOperationException("Failed to deserialize access event response");
    }
}
