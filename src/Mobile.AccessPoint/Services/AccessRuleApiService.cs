using Microsoft.Extensions.Logging;
using Shared.DTOs;
using System.Net.Http.Json;

namespace Mobile.AccessPoint.Services;

/// <summary>
/// API service for downloading access rules from backend
/// </summary>
public class AccessRuleApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccessRuleApiService> _logger;

    public AccessRuleApiService(HttpClient httpClient, ILogger<AccessRuleApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Downloads all active access rules from backend
    /// </summary>
    public async Task<List<AccessRuleDto>> GetAccessRulesAsync()
    {
        try
        {
            _logger.LogInformation("Downloading access rules from backend");

            var response = await _httpClient.GetAsync("/api/access-events/rules");
            response.EnsureSuccessStatusCode();

            var rules = await response.Content.ReadFromJsonAsync<List<AccessRuleDto>>();
            
            _logger.LogInformation("Downloaded {Count} access rules", rules?.Count ?? 0);
            
            return rules ?? new List<AccessRuleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading access rules");
            throw;
        }
    }
}

