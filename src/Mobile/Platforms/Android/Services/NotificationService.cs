using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Mobile.Services;

namespace Mobile.Platforms.Android.Services;

public class NotificationService : INotificationService
{
    private const string ChannelId = "connectivity_channel";
    private const string ChannelName = "Notificaciones de Conectividad";
    private const string ChannelDescription = "Notificaciones sobre el estado de la conexión";
    private int _notificationId = 1000;

    public NotificationService()
    {
        CreateNotificationChannel();
    }

    public Task ShowNotification(string title, string message)
    {
        var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;
        
        var notificationBuilder = new NotificationCompat.Builder(context, ChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Resource.Drawable.dotnet_bot) // Icono de la app
            .SetPriority(NotificationCompat.PriorityDefault)
            .SetAutoCancel(true)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(message));

        var notificationManager = NotificationManagerCompat.From(context);
        
        // Verificar permisos en Android 13+
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            if (context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) 
                == global::Android.Content.PM.Permission.Granted)
            {
                notificationManager.Notify(_notificationId++, notificationBuilder.Build());
            }
        }
        else
        {
            notificationManager.Notify(_notificationId++, notificationBuilder.Build());
        }

        return Task.CompletedTask;
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Default)
            {
                Description = ChannelDescription
            };

            var notificationManager = (NotificationManager?)global::Android.App.Application.Context.GetSystemService(global::Android.Content.Context.NotificationService);
            notificationManager?.CreateNotificationChannel(channel);
        }
    }
}
