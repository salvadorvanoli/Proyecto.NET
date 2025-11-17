using Application.Notifications.DTOs;

namespace Application.Notifications.Services;

/// <summary>
/// Service interface for managing notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gets all notifications for a specific user.
    /// </summary>
    Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all notifications for the current tenant.
    /// </summary>
    Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a notification by ID.
    /// </summary>
    Task<NotificationResponseDto?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task<NotificationResponseDto> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    Task<int> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
}
