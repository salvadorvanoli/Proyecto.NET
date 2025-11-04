using Application.SpaceTypes.DTOs;

namespace Application.SpaceTypes.Services;

/// <summary>
/// Service interface for space type operations.
/// </summary>
public interface ISpaceTypeService
{
    /// <summary>
    /// Creates a new space type in the current tenant context.
    /// </summary>
    Task<SpaceTypeResponse> CreateSpaceTypeAsync(CreateSpaceTypeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a space type by ID.
    /// </summary>
    Task<SpaceTypeResponse?> GetSpaceTypeByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all space types from the current tenant.
    /// </summary>
    Task<IEnumerable<SpaceTypeResponse>> GetSpaceTypesByTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing space type.
    /// </summary>
    Task<SpaceTypeResponse> UpdateSpaceTypeAsync(int id, UpdateSpaceTypeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a space type by ID.
    /// </summary>
    Task<bool> DeleteSpaceTypeAsync(int id, CancellationToken cancellationToken = default);
}
