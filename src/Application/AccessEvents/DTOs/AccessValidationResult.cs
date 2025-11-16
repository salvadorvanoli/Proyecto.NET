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
    public static AccessValidationResult Granted(string userName, string controlPointName, string reason = "Acceso permitido")
    {
        return new AccessValidationResult
        {
            IsGranted = true,
            Reason = reason,
            UserName = userName,
            ControlPointName = controlPointName
        };
    }
    
    /// <summary>
    /// Creates a denied access result.
    /// </summary>
    public static AccessValidationResult Denied(string userName, string controlPointName, string reason)
    {
        return new AccessValidationResult
        {
            IsGranted = false,
            Reason = reason,
            UserName = userName,
            ControlPointName = controlPointName
        };
    }
}
