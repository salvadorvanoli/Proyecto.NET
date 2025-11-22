namespace Application.Notifications.DTOs;

/// <summary>
/// Request DTO for marking a notification as read.
/// </summary>
public class MarkAsReadRequest
{
    public int NotificationId { get; set; }
}
