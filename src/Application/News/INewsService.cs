using Shared.DTOs.News;

namespace Application.News;

/// <summary>
/// Interface for news service operations.
/// </summary>
public interface INewsService
{
    Task<NewsResponse> CreateNewsAsync(NewsRequest request, CancellationToken cancellationToken = default);
    Task<NewsResponse?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsResponse>> GetAllNewsAsync(CancellationToken cancellationToken = default);
    Task<NewsResponse> UpdateNewsAsync(int id, NewsRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default);
}


