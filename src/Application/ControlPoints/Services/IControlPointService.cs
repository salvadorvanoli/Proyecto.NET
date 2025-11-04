using Application.ControlPoints.DTOs;

namespace Application.ControlPoints.Services;

/// <summary>
/// Service interface for control point operations.
/// </summary>
public interface IControlPointService
{
    /// <summary>
    /// Creates a new control point in the current tenant context.
    /// </summary>
    Task<ControlPointResponse> CreateControlPointAsync(CreateControlPointRequest request, CancellationToken cancellationToken = default);

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
    Task<ControlPointResponse> UpdateControlPointAsync(int id, UpdateControlPointRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a control point by ID.
    /// </summary>
    Task<bool> DeleteControlPointAsync(int id, CancellationToken cancellationToken = default);
}
