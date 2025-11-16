namespace Shared.DTOs.AccessEvents;

/// <summary>
/// Result of access validation.
/// </summary>
public class AccessValidationResult
{
    /// <summary>
    /// Indicates if access is granted.
    /// </summary>
    public bool IsGranted { get; set; }
    
    /// <summary>
    /// The reason for the access decision.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// The access result as string ("Granted" or "Denied").
    /// </summary>
    public string Result => IsGranted ? "Granted" : "Denied";
    
    /// <summary>
    /// User's full name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Control point name.
    /// </summary>
    public string ControlPointName { get; set; } = string.Empty;
}
