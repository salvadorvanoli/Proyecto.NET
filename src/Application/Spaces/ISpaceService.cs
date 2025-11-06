using Shared.DTOs.Spaces;

namespace Application.Spaces;

/// <summary>
/// Service interface for space management.
/// </summary>
public interface ISpaceService
{
    /// <summary>
    /// Creates a new space in the current tenant context.
    /// </summary>
    Task<SpaceResponse> CreateSpaceAsync(SpaceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a space by ID.
    /// </summary>
    Task<SpaceResponse?> GetSpaceByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all spaces from the current tenant.
    /// </summary>
    Task<IEnumerable<SpaceResponse>> GetSpacesByTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing space.
    /// </summary>
    Task<SpaceResponse> UpdateSpaceAsync(int id, SpaceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a space by ID.
    /// </summary>
    Task<bool> DeleteSpaceAsync(int id, CancellationToken cancellationToken = default);
}
