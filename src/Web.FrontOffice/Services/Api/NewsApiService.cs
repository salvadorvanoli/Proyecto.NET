using System.Net.Http.Json;
using Shared.DTOs.News;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of news API service.
/// </summary>
public class NewsApiService : INewsApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsApiService> _logger;

    public NewsApiService(HttpClient httpClient, ILogger<NewsApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<NewsResponse>> GetAllNewsAsync()
    {
        try
        {
            // Add X-Tenant-Id header for default tenant (1)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var response = await _httpClient.GetAsync("api/news");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch news with status code: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<NewsResponse>();
            }

            var news = await response.Content.ReadFromJsonAsync<IEnumerable<NewsResponse>>();
            return news ?? Enumerable.Empty<NewsResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news from API");
            return Enumerable.Empty<NewsResponse>();
        }
    }

    public async Task<NewsResponse?> GetNewsByIdAsync(int id)
    {
        try
        {
            // Add X-Tenant-Id header for default tenant (1)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var response = await _httpClient.GetAsync($"api/news/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch news {Id} with status code: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var news = await response.Content.ReadFromJsonAsync<NewsResponse>();
            return news;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news {Id} from API", id);
            return null;
        }
    }
}
