using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;

namespace Web.FrontOffice.Services;

/// <summary>
/// Servicio simple para manejar la conexión de SignalR
/// </summary>
public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _apiUrl;
    private readonly ILogger<SignalRService> _logger;
    private int _currentUserId = 0;
    private bool _isConnecting = false;

    public event Action<int, string, string, int>? OnNotificationReceived;

    public SignalRService(IConfiguration configuration, ILogger<SignalRService> logger)
    {
        _apiUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5236/";
        _logger = logger;
    }

    public async Task StartConnectionAsync(int userId)
    {
        // Si ya estamos conectados con el mismo usuario, no hacer nada
        if (_hubConnection?.State == HubConnectionState.Connected && _currentUserId == userId)
        {
            _logger.LogInformation("Ya conectado a SignalR para userId: {UserId}", userId);
            return;
        }

        // Si ya estamos en proceso de conectar, esperar
        if (_isConnecting)
        {
            _logger.LogInformation("Conexión a SignalR ya en progreso");
            return;
        }

        _isConnecting = true;

        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }

            _currentUserId = userId;

            // Construir la URL del hub con el userId como query parameter
            // IMPORTANTE: Incluir /api porque el hub está en la API que está detrás de /api en el ALB
            var apiBaseUrl = _apiUrl.TrimEnd('/');
            var hubUrl = $"{apiBaseUrl}/notificationHub?userId={userId}";

            _logger.LogInformation("Intentando conectar a SignalR: {HubUrl}", hubUrl);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    // Configurar transportes múltiples para mejor compatibilidad con ALB
                    // Intentará WebSockets primero, luego ServerSentEvents, y finalmente LongPolling
                    options.Transports = HttpTransportType.WebSockets |
                                        HttpTransportType.ServerSentEvents |
                                        HttpTransportType.LongPolling;

                    // Configurar timeouts más largos para conexiones a través de ALB
                    options.CloseTimeout = TimeSpan.FromSeconds(60);

                    // Habilitar cookies para sticky sessions del ALB
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                        {
                            clientHandler.UseCookies = true;
                            clientHandler.UseDefaultCredentials = false;
                        }
                        return handler;
                    };

                    // Agregar headers adicionales si es necesario
                    options.Headers["X-Requested-With"] = "SignalR";
                })
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30)
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            // Escuchar el evento "ReceiveNotification"
            _hubConnection.On<NotificationData>("ReceiveNotification", (notification) =>
            {
                _logger.LogInformation("Notificación recibida via SignalR: {Title}", notification.title);
                OnNotificationReceived?.Invoke(
                    notification.id,
                    notification.title,
                    notification.message,
                    notification.id
                );
            });

            // Eventos de conexión para debugging
            _hubConnection.Closed += async (error) =>
            {
                _logger.LogWarning("Conexión SignalR cerrada: {Error}", error?.Message ?? "Sin error");
                await Task.CompletedTask;
            };

            _hubConnection.Reconnecting += (error) =>
            {
                _logger.LogWarning("Reconectando SignalR: {Error}", error?.Message ?? "Sin error");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                _logger.LogInformation("SignalR reconectado: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            await _hubConnection.StartAsync();
            _logger.LogInformation("✅ Conexión SignalR establecida exitosamente para userId: {UserId}, ConnectionId: {ConnectionId}",
                userId, _hubConnection.ConnectionId ?? "N/A");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al conectar a SignalR");
        }
        finally
        {
            _isConnecting = false;
        }
    }

    public async Task StopConnectionAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
            }
            catch
            {
                // Ignorar errores al desconectar
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    // Clase interna para deserializar la notificación
    private class NotificationData
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public bool isRead { get; set; }
    }
}
