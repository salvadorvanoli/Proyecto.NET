using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Web.Api.Hubs;

namespace Web.Api.Services;

/// <summary>
/// SignalR implementation of notification hub service
/// </summary>
public class SignalRNotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(int userId, string title, string message, int notificationId)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("ReceiveNotification", new
            {
                id = notificationId,
                title,
                message,
                createdAt = DateTime.UtcNow,
                isRead = false
            });
    }

    public async Task SendNotificationToUsersAsync(IEnumerable<int> userIds, string title, string message)
    {
        var groups = userIds.Select(id => $"user_{id}").ToList();
        
        await _hubContext.Clients
            .Groups(groups)
            .SendAsync("ReceiveNotification", new
            {
                title,
                message,
                createdAt = DateTime.UtcNow,
                isRead = false
            });
    }
}
