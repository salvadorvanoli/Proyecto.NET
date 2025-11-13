using Application.News.DTOs;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Interface for news API service.
/// </summary>
public interface INewsApiService
{
    Task<IEnumerable<NewsResponseDto>> GetAllNewsAsync();
}
