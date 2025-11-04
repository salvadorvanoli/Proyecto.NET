using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing space types through the API.
/// </summary>
public interface ISpaceTypeApiService
{
    Task<IEnumerable<SpaceTypeDto>> GetAllSpaceTypesAsync();
    Task<SpaceTypeDto?> GetSpaceTypeByIdAsync(int id);
    Task<SpaceTypeDto> CreateSpaceTypeAsync(CreateSpaceTypeDto createSpaceTypeDto);
    Task<SpaceTypeDto> UpdateSpaceTypeAsync(int id, UpdateSpaceTypeDto updateSpaceTypeDto);
    Task<bool> DeleteSpaceTypeAsync(int id);
}
