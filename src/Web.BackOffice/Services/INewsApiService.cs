using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing news through the API.
/// </summary>
public interface INewsApiService
{
    Task<IEnumerable<NewsDto>> GetAllNewsAsync();
    Task<NewsDto?> GetNewsByIdAsync(int id);
    Task<NewsDto> CreateNewsAsync(CreateNewsDto createNewsDto);
    Task<NewsDto> UpdateNewsAsync(int id, UpdateNewsDto updateNewsDto);
    Task<bool> DeleteNewsAsync(int id);
}

