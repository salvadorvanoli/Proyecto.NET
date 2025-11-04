using Application.Spaces.DTOs;

namespace Application.Spaces.Services;

/// <summary>
/// Service interface for space operations.
/// </summary>
public interface ISpaceService
{
    /// <summary>
    /// Creates a new space in the current tenant context.
    /// </summary>
    Task<SpaceResponse> CreateSpaceAsync(CreateSpaceRequest request, CancellationToken cancellationToken = default);

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
    Task<SpaceResponse> UpdateSpaceAsync(int id, UpdateSpaceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a space by ID.
    /// </summary>
    Task<bool> DeleteSpaceAsync(int id, CancellationToken cancellationToken = default);
}
