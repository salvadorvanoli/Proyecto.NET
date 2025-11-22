using Shared.DTOs.AccessEvents;

namespace Application.AccessEvents;

/// <summary>
/// Service interface for validating user access to control points.
/// </summary>
public interface IAccessValidationService
{
    /// <summary>
    /// Validates if a user has access to a specific control point at the given time.
    /// </summary>
    /// <param name="userId">The user ID attempting access.</param>
    /// <param name="controlPointId">The control point ID to access.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result indicating if access is granted and the reason.</returns>
    Task<AccessValidationResult> ValidateAccessAsync(int userId, int controlPointId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates if a credential has access to a specific control point at the given time.
    /// </summary>
    /// <param name="credentialId">The credential ID attempting access.</param>
    /// <param name="controlPointId">The control point ID to access.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result indicating if access is granted and the reason.</returns>
    Task<AccessValidationResult> ValidateAccessByCredentialAsync(int credentialId, int controlPointId, CancellationToken cancellationToken = default);
}
