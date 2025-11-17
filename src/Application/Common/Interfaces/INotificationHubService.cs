namespace Application.Common.Interfaces;

/// <summary>
/// Interface for sending real-time notifications via SignalR
/// </summary>
public interface INotificationHubService
{
    /// <summary>
    /// Send a notification to a specific user
    /// </summary>
    /// <param name="userId">The user ID to send the notification to</param>
    /// <param name="title">The notification title</param>
    /// <param name="message">The notification message</param>
    /// <param name="notificationId">The notification ID from database</param>
    Task SendNotificationToUserAsync(int userId, string title, string message, int notificationId);
    
    /// <summary>
    /// Send a notification to multiple users
    /// </summary>
    /// <param name="userIds">List of user IDs to send the notification to</param>
    /// <param name="title">The notification title</param>
    /// <param name="message">The notification message</param>
    Task SendNotificationToUsersAsync(IEnumerable<int> userIds, string title, string message);
}
