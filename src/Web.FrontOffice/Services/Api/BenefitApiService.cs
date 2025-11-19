using AppBenefitResponse = Application.Benefits.DTOs.BenefitResponse;
using Shared.DTOs.Benefits;
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

    public async Task<List<AppBenefitResponse>> GetUserBenefitsAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"api/benefits/user/{userId}");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<AppBenefitResponse>>();
        return benefits ?? new List<AppBenefitResponse>();
    }

    public async Task<(List<AppBenefitResponse> Benefits, int TotalCount)> GetUserBenefitsPagedAsync(
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

    public async Task<List<AppBenefitResponse>> GetAllBenefitsAsync()
    {
        var response = await _httpClient.GetAsync("api/benefits");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<AppBenefitResponse>>();
        return benefits ?? new List<AppBenefitResponse>();
    }

    public async Task<AppBenefitResponse?> GetBenefitByIdAsync(int benefitId)
    {
        var response = await _httpClient.GetAsync($"api/benefits/{benefitId}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AppBenefitResponse>();
    }

    public async Task<List<AppBenefitResponse>> GetActiveBenefitsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/benefits/by-tenant");
            response.EnsureSuccessStatusCode();

            var benefits = await response.Content.ReadFromJsonAsync<List<AppBenefitResponse>>();
            return benefits ?? new List<AppBenefitResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active benefits");
            throw;
        }
    }

    public async Task<ConsumeBenefitResponse> ConsumeBenefitAsync(ConsumeBenefitRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/benefits/consume", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error consuming benefit: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                
                // Intentar extraer el mensaje de error del JSON
                string errorMessage = "Error al consumir el beneficio.";
                try
                {
                    var errorObject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                    if (errorObject != null && errorObject.ContainsKey("error"))
                    {
                        errorMessage = errorObject["error"].ToString() ?? errorMessage;
                    }
                }
                catch
                {
                    // Si falla el parseo, usar el mensaje por defecto
                }
                
                throw new HttpRequestException(errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<ConsumeBenefitResponse>();
            
            if (result == null)
                throw new InvalidOperationException("La respuesta del servidor no contiene datos válidos.");

            _logger.LogInformation("Benefit {BenefitId} consumed successfully. Usage ID: {UsageId}", 
                request.BenefitId, result.UsageId);

            return result;
        }
        catch (HttpRequestException)
        {
            throw; // Re-lanzar HttpRequestException con el mensaje personalizado
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming benefit {BenefitId}", request.BenefitId);
            throw;
        }
    }
}
