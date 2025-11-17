using Application.Notifications.DTOs;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Interface for notification API service.
/// </summary>
public interface INotificationApiService
{
    Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId);
    Task<NotificationResponseDto?> MarkAsReadAsync(int notificationId);
    Task<int> MarkAllAsReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
}
