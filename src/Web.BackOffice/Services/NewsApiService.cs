using System.Net.Http.Json;
using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Implementation of news API service using HttpClient.
/// </summary>
public class NewsApiService : INewsApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsApiService> _logger;
    private const string BaseUrl = "api/news";

    public NewsApiService(HttpClient httpClient, ILogger<NewsApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<NewsDto>> GetAllNewsAsync()
    {
        try
        {
            var news = await _httpClient.GetFromJsonAsync<IEnumerable<NewsDto>>(BaseUrl);
            return news ?? Enumerable.Empty<NewsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving news from API");
            throw;
        }
    }

    public async Task<NewsDto?> GetNewsByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<NewsDto>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving news {NewsId} from API", id);
            throw;
        }
    }

    public async Task<NewsDto> CreateNewsAsync(CreateNewsDto createNewsDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, createNewsDto);
            response.EnsureSuccessStatusCode();

            var news = await response.Content.ReadFromJsonAsync<NewsDto>();
            return news ?? throw new InvalidOperationException("Failed to deserialize news response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news via API");
            throw;
        }
    }

    public async Task<NewsDto> UpdateNewsAsync(int id, UpdateNewsDto updateNewsDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateNewsDto);
            response.EnsureSuccessStatusCode();

            var news = await response.Content.ReadFromJsonAsync<NewsDto>();
            return news ?? throw new InvalidOperationException("Failed to deserialize news response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news {NewsId} via API", id);
            throw;
        }
    }

    public async Task<bool> DeleteNewsAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news {NewsId} via API", id);
            throw;
        }
    }
}

