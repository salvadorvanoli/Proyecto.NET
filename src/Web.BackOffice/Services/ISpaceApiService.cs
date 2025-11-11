using Shared.DTOs.Spaces;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing spaces through the API.
/// </summary>
public interface ISpaceApiService
{
    Task<IEnumerable<SpaceResponse>> GetSpacesByTenantAsync();
    Task<SpaceResponse?> GetSpaceByIdAsync(int id);
    Task<SpaceResponse> CreateSpaceAsync(SpaceRequest createSpaceDto);
    Task<SpaceResponse> UpdateSpaceAsync(int id, SpaceRequest updateSpaceDto);
    Task<bool> DeleteSpaceAsync(int id);
}
