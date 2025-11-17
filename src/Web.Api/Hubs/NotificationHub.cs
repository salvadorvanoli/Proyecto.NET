using Microsoft.AspNetCore.SignalR;

namespace Web.Api.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        
        _logger.LogInformation("🔌 Cliente conectado. ConnectionId: {ConnectionId}, UserId: {UserId}", 
            Context.ConnectionId, userId ?? "NULL");
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Agregar el usuario a un grupo basado en su ID
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("✅ Usuario {UserId} agregado al grupo user_{UserId}", userId, userId);
        }
        else
        {
            _logger.LogWarning("⚠️ Conexión sin userId en query string");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        
        _logger.LogInformation("🔌 Cliente desconectado. ConnectionId: {ConnectionId}, UserId: {UserId}", 
            Context.ConnectionId, userId ?? "NULL");
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
