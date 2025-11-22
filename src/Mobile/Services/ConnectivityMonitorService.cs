namespace Mobile.Services;

public class ConnectivityMonitorService : IConnectivityMonitorService
{
    private readonly INotificationService _notificationService;
    private bool _wasDisconnected = false;

    public ConnectivityMonitorService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void StartMonitoring()
    {
        // Suscribirse a cambios de conectividad
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        
        // Verificar el estado inicial
        _wasDisconnected = Connectivity.NetworkAccess != NetworkAccess.Internet;
    }

    public void StopMonitoring()
    {
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        // Si ahora tenemos conexión a internet y antes estábamos desconectados
        if (e.NetworkAccess == NetworkAccess.Internet && _wasDisconnected)
        {
            _wasDisconnected = false;
            
            // Mostrar notificación
            await _notificationService.ShowNotification(
                "Conexión Restaurada",
                "Tu dispositivo volvió a conectarse. La app verificó y aplicó las últimas actualizaciones."
            );
        }
        else if (e.NetworkAccess != NetworkAccess.Internet)
        {
            _wasDisconnected = true;
        }
    }
}
