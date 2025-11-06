using Shared.DTOs.News;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing news through the API.
/// </summary>
public interface INewsApiService
{
    Task<IEnumerable<NewsResponse>> GetAllNewsAsync();
    Task<NewsResponse?> GetNewsByIdAsync(int id);
    Task<NewsResponse> CreateNewsAsync(NewsRequest createNewsDto);
    Task<NewsResponse> UpdateNewsAsync(int id, NewsRequest updateNewsDto);
    Task<bool> DeleteNewsAsync(int id);
}

