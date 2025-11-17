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
    private readonly ILogger<SignalRNotificationHubService> _logger;

    public SignalRNotificationHubService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(int userId, string title, string message, int notificationId)
    {
        _logger.LogInformation("📤 Enviando notificación via SignalR a user_{UserId}: {Title}", userId, title);
        
        try
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
            
            _logger.LogInformation("✅ Notificación enviada exitosamente a user_{UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar notificación via SignalR a user_{UserId}", userId);
        }
    }

    public async Task SendNotificationToUsersAsync(IEnumerable<int> userIds, string title, string message)
    {
        var groups = userIds.Select(id => $"user_{id}").ToList();
        
        await _hubContext.Clients
            .Groups(groups)
            .SendAsync("ReceiveNotification", new
            {
                id = 0, // Sin ID específico al enviar a múltiples usuarios
                title,
                message,
                createdAt = DateTime.UtcNow,
                isRead = false
            });
    }
}
