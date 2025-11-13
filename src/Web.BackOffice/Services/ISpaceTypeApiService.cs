using Shared.DTOs.SpaceTypes;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing space types through the API.
/// </summary>
public interface ISpaceTypeApiService
{
    Task<IEnumerable<SpaceTypeResponse>> GetSpaceTypesByTenantAsync();
    Task<SpaceTypeResponse?> GetSpaceTypeByIdAsync(int id);
    Task<SpaceTypeResponse> CreateSpaceTypeAsync(SpaceTypeRequest request);
    Task<SpaceTypeResponse> UpdateSpaceTypeAsync(int id, SpaceTypeRequest request);
    Task<bool> DeleteSpaceTypeAsync(int id);
}
