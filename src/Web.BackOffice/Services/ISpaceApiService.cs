using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing spaces through the API.
/// </summary>
public interface ISpaceApiService
{
    Task<IEnumerable<SpaceDto>> GetAllSpacesAsync();
    Task<SpaceDto?> GetSpaceByIdAsync(int id);
    Task<SpaceDto> CreateSpaceAsync(CreateSpaceDto createSpaceDto);
    Task<SpaceDto> UpdateSpaceAsync(int id, UpdateSpaceDto updateSpaceDto);
    Task<bool> DeleteSpaceAsync(int id);
}
