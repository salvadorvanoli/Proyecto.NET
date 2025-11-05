using Shared.DTOs.Requests;
using Shared.DTOs.Responses;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing space types through the API.
/// </summary>
public interface ISpaceTypeApiService
{
    Task<IEnumerable<SpaceTypeResponse>> GetAllSpaceTypesAsync();
    Task<SpaceTypeResponse?> GetSpaceTypeByIdAsync(int id);
    Task<SpaceTypeResponse> CreateSpaceTypeAsync(SpaceTypeRequest request);
    Task<SpaceTypeResponse> UpdateSpaceTypeAsync(int id, SpaceTypeRequest request);
    Task<bool> DeleteSpaceTypeAsync(int id);
}
