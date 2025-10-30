using Application.News.DTOs;

namespace Application.News.Services;

/// <summary>
/// Interface for news service operations.
/// </summary>
public interface INewsService
{
    Task<NewsResponseDto> CreateNewsAsync(CreateNewsRequest request, CancellationToken cancellationToken = default);
    Task<NewsResponseDto?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsResponseDto>> GetAllNewsAsync(CancellationToken cancellationToken = default);
    Task<NewsResponseDto> UpdateNewsAsync(int id, UpdateNewsRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default);
}

