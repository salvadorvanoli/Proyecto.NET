using Microsoft.AspNetCore.SignalR;

namespace Web.Api.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Agregar el usuario a un grupo basado en su ID
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
