using Application.Benefits.DTOs;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of the benefit API service.
/// </summary>
public class BenefitApiService : IBenefitApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BenefitApiService> _logger;

    public BenefitApiService(HttpClient httpClient, ILogger<BenefitApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"api/benefits/user/{userId}");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<BenefitResponse>>();
        return benefits ?? new List<BenefitResponse>();
    }

    public async Task<(List<BenefitResponse> Benefits, int TotalCount)> GetUserBenefitsPagedAsync(
        int userId, 
        int skip = 0, 
        int take = 10, 
        string? searchTerm = null)
    {
        try
        {
            // Get all benefits for the user
            var allBenefits = await GetUserBenefitsAsync(userId);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allBenefits = allBenefits
                    .Where(b => 
                        b.BenefitType.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.BenefitType.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalCount = allBenefits.Count;

            // Apply pagination
            var paginatedBenefits = allBenefits
                .Skip(skip)
                .Take(take)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} benefits for user {UserId} (total: {Total}, skip: {Skip}, take: {Take}, search: '{Search}')", 
                paginatedBenefits.Count, userId, totalCount, skip, take, searchTerm ?? "none");

            return (paginatedBenefits, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated benefits for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<BenefitResponse>> GetAllBenefitsAsync()
    {
        var response = await _httpClient.GetAsync("api/benefits");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<BenefitResponse>>();
        return benefits ?? new List<BenefitResponse>();
    }

    public async Task<BenefitResponse?> GetBenefitByIdAsync(int benefitId)
    {
        var response = await _httpClient.GetAsync($"api/benefits/{benefitId}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BenefitResponse>();
    }
}
