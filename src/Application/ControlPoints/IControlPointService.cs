using Shared.DTOs.ControlPoints;

namespace Application.ControlPoints;

/// <summary>
/// Service interface for control point management.
/// </summary>
public interface IControlPointService
{
    /// <summary>
    /// Creates a new control point in the current tenant context.
    /// </summary>
    Task<ControlPointResponse> CreateControlPointAsync(ControlPointRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a control point by ID.
    /// </summary>
    Task<ControlPointResponse?> GetControlPointByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all control points from the current tenant.
    /// </summary>
    Task<IEnumerable<ControlPointResponse>> GetControlPointsByTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing control point.
    /// </summary>
    Task<ControlPointResponse> UpdateControlPointAsync(int id, ControlPointRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a control point by ID.
    /// </summary>
    Task<bool> DeleteControlPointAsync(int id, CancellationToken cancellationToken = default);
}
