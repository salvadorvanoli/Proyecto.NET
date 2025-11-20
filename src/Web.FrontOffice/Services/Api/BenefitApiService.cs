using Shared.DTOs.Benefits;
using Web.FrontOffice.Services.Interfaces;
using BenefitDto = Shared.DTOs.Benefits.BenefitResponse;

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

    public async Task<List<BenefitDto>> GetUserBenefitsAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"api/benefits/user/{userId}");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<BenefitDto>>();
        return benefits ?? new List<BenefitDto>();
    }

    public async Task<(List<BenefitDto> Benefits, int TotalCount)> GetUserBenefitsPagedAsync(
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
                        b.BenefitTypeName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
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

    public async Task<List<BenefitDto>> GetAllBenefitsAsync()
    {
        var response = await _httpClient.GetAsync("api/benefits");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<BenefitDto>>();
        return benefits ?? new List<BenefitDto>();
    }

    public async Task<BenefitDto?> GetBenefitByIdAsync(int benefitId)
    {
        var response = await _httpClient.GetAsync($"api/benefits/{benefitId}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BenefitDto>();
    }

    public async Task<RedeemBenefitResponse> RedeemBenefitAsync(RedeemBenefitRequest request)
    {
        try
        {
            _logger.LogInformation("Redeeming benefit {BenefitId} for user {UserId}",
                request.BenefitId, request.UserId);

            var response = await _httpClient.PostAsJsonAsync("api/benefits/redeem", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to redeem benefit. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Error al canjear el beneficio: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<RedeemBenefitResponse>();
            
            if (result == null)
                throw new InvalidOperationException("La respuesta del servidor fue nula.");

            _logger.LogInformation("Successfully redeemed benefit {BenefitId} for user {UserId}",
                request.BenefitId, request.UserId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming benefit {BenefitId} for user {UserId}",
                request.BenefitId, request.UserId);
            throw;
        }
    }
}
