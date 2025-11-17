using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Default implementation that does nothing - will be replaced in Web.Api
/// </summary>
public class NotificationHubService : INotificationHubService
{
    public virtual Task SendNotificationToUserAsync(int userId, string title, string message, int notificationId)
    {
        // This will be overridden by SignalRNotificationHubService in Web.Api
        return Task.CompletedTask;
    }

    public virtual Task SendNotificationToUsersAsync(IEnumerable<int> userIds, string title, string message)
    {
        // This will be overridden by SignalRNotificationHubService in Web.Api
        return Task.CompletedTask;
    }
}
