namespace Application.AccessEvents.DTOs;

/// <summary>
/// Request DTO for creating a new access event.
/// </summary>
public class CreateAccessEventRequest
{
    /// <summary>
    /// ID of the control point where the access attempt occurred.
    /// </summary>
    public int ControlPointId { get; set; }

    /// <summary>
    /// ID of the user attempting access.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Result of the access attempt ("Granted" or "Denied").
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Date and time of the event. If not provided, current time is used.
    /// </summary>
    public DateTime? EventDateTime { get; set; }
}
