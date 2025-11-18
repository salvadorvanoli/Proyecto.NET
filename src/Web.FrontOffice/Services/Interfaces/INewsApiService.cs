using Shared.DTOs.News;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Interface for news API service.
/// </summary>
public interface INewsApiService
{
    Task<IEnumerable<NewsResponse>> GetAllNewsAsync();
}
