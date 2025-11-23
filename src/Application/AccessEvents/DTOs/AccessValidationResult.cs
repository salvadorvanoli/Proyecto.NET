using Shared.DTOs.AccessEvents;

namespace Application.AccessEvents.DTOs;

/// <summary>
/// Extension methods for AccessValidationResult.
/// </summary>
public static class AccessValidationResultExtensions
{
    /// <summary>
    /// Creates a granted access result.
    /// </summary>
    public static AccessValidationResult Granted(int userId, string userName, string controlPointName, string spaceName, string reason = "Acceso permitido")
    {
        return new AccessValidationResult
        {
            IsGranted = true,
            Reason = reason,
            UserId = userId,
            UserName = userName,
            ControlPointName = controlPointName,
            SpaceName = spaceName
        };
    }
    
    /// <summary>
    /// Creates a denied access result.
    /// </summary>
    public static AccessValidationResult Denied(int userId, string userName, string controlPointName, string spaceName, string reason)
    {
        return new AccessValidationResult
        {
            IsGranted = false,
            Reason = reason,
            UserId = userId,
            UserName = userName,
            ControlPointName = controlPointName,
            SpaceName = spaceName
        };
    }
}
