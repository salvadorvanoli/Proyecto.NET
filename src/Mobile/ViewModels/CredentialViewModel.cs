using System.Windows.Input;
using Mobile.Services;
using Microsoft.Extensions.Logging;
using Shared.DTOs.Auth;
using Mobile.Models;
using CommunityToolkit.Maui.Views;
using Mobile.Popups;
using CommunityToolkit.Mvvm.Messaging;
using Mobile.Messages;

namespace Mobile.ViewModels;

/// <summary>
/// ViewModel for the digital credential view
/// Handles NFC credential emulation (HCE mode)
/// </summary>
public class CredentialViewModel : BaseViewModel
{
    private readonly INfcCredentialService _nfcCredentialService;
    private readonly IAuthService _authService;
    private readonly IAccessEventService _accessEventService;
    private readonly ISyncService _syncService;
    private readonly ILogger<CredentialViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private int? _userId;
    private int? _credentialId;
    private string _userName = "Usuario";
    private string _roles = "";
    private bool _isEmulating;
    private bool _isHceAvailable;
    private string _statusMessage = "Cargando credencial...";
    private bool _isOnline;
    private string _connectivityStatusText = "Verificando...";
    private string _connectivityStatusIcon = "📡";
    private Color _connectivityStatusColor = Colors.Gray;
    private string _activationButtonText = "🚀 Activar Credencial";
    private Color _activationButtonColor = Colors.Green;
    private bool _showAccessResult;
    private string _accessResultIcon = "✅";
    private string _accessResultTitle = "Acceso Concedido";
    private string _accessResultMessage = "";
    private Color _accessResultTextColor = Colors.Green;
    private Color _accessResultBorderColor = Colors.Green;

    public int? UserId
    {
        get => _userId;
        set => SetProperty(ref _userId, value);
    }

    public int? CredentialId
    {
        get => _credentialId;
        set => SetProperty(ref _credentialId, value);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Roles
    {
        get => _roles;
        set => SetProperty(ref _roles, value);
    }

    public bool IsEmulating
    {
        get => _isEmulating;
        set => SetProperty(ref _isEmulating, value);
    }

    public bool IsHceAvailable
    {
        get => _isHceAvailable;
        set => SetProperty(ref _isHceAvailable, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsOnline
    {
        get => _isOnline;
        set => SetProperty(ref _isOnline, value);
    }

    public string ConnectivityStatusText
    {
        get => _connectivityStatusText;
        set => SetProperty(ref _connectivityStatusText, value);
    }

    public string ConnectivityStatusIcon
    {
        get => _connectivityStatusIcon;
        set => SetProperty(ref _connectivityStatusIcon, value);
    }

    public Color ConnectivityStatusColor
    {
        get => _connectivityStatusColor;
        set => SetProperty(ref _connectivityStatusColor, value);
    }

    public string ActivationButtonText
    {
        get => _activationButtonText;
        set => SetProperty(ref _activationButtonText, value);
    }

    public Color ActivationButtonColor
    {
        get => _activationButtonColor;
        set => SetProperty(ref _activationButtonColor, value);
    }

    public bool ShowAccessResult
    {
        get => _showAccessResult;
        set => SetProperty(ref _showAccessResult, value);
    }

    public string AccessResultIcon
    {
        get => _accessResultIcon;
        set => SetProperty(ref _accessResultIcon, value);
    }

    public string AccessResultTitle
    {
        get => _accessResultTitle;
        set => SetProperty(ref _accessResultTitle, value);
    }

    public string AccessResultMessage
    {
        get => _accessResultMessage;
        set => SetProperty(ref _accessResultMessage, value);
    }

    public Color AccessResultTextColor
    {
        get => _accessResultTextColor;
        set => SetProperty(ref _accessResultTextColor, value);
    }

    public Color AccessResultBorderColor
    {
        get => _accessResultBorderColor;
        set => SetProperty(ref _accessResultBorderColor, value);
    }

    public ICommand StartEmulationCommand { get; }
    public ICommand StopEmulationCommand { get; }
    public ICommand ToggleEmulationCommand { get; }
    public ICommand LogoutCommand { get; }

    public CredentialViewModel(
        INfcCredentialService nfcCredentialService,
        IAuthService authService,
        IAccessEventService accessEventService,
        ISyncService syncService,
        ILogger<CredentialViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _nfcCredentialService = nfcCredentialService;
        _authService = authService;
        _accessEventService = accessEventService;
        _syncService = syncService;
        _logger = logger;
        _navigationService = navigationService;
        _dialogService = dialogService;

        Title = "Mi Credencial Digital";

        StartEmulationCommand = new Command(async () => await StartEmulation());
        StopEmulationCommand = new Command(StopEmulation);
        ToggleEmulationCommand = new Command(async () => await ToggleEmulation());
        LogoutCommand = new Command(async () => await Logout());
        
        // Subscribe to connectivity changes
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        UpdateConnectivityStatus();
        
        // Subscribe to NFC access responses from control point
        _nfcCredentialService.AccessResponseReceived += OnAccessResponseReceived;
        
        // Load user credentials automatically
        Task.Run(async () => await LoadUserCredentials());
    }
    
    private async void OnAccessResponseReceived(object? sender, AccessResponse response)
    {
        try
        {
            _logger.LogInformation("🎯 Access response: {Granted} - {Message}", 
                response.AccessGranted, response.Message);

            // Guardar evento localmente (especialmente importante en modo offline)
            try
            {
                var accessEvent = new AccessEventDto
                {
                    UserId = UserId ?? 0,
                    ControlPointId = response.ControlPointId ?? 0,
                    ControlPointName = response.ControlPointName ?? "Punto de Control",
                    SpaceName = "", // Se actualizar\u00e1 cuando se sincronice con el servidor
                    Timestamp = DateTime.UtcNow,
                    WasGranted = response.AccessGranted,
                    DenialReason = response.AccessGranted ? null : response.Message
                };

                await _accessEventService.SaveAccessEventAsync(accessEvent);
                _logger.LogInformation("✅ Evento de acceso guardado localmente - ControlPoint: {Name}", accessEvent.ControlPointName);

                // Notificar al historial para que se actualice
                _logger.LogInformation("📢 ENVIANDO MENSAJE: AccessEventCreated desde CredentialViewModel");
                WeakReferenceMessenger.Default.Send(new AccessEventCreatedMessage());
                _logger.LogInformation("📢 MENSAJE ENVIADO: AccessEventCreated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando evento de acceso localmente");
            }

            // Mostrar card de resultado en la página
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    _logger.LogInformation("🎨 Mostrando card de resultado...");
                    
                    // Configurar la card según el resultado
                    if (response.AccessGranted)
                    {
                        AccessResultIcon = "✅";
                        AccessResultTitle = "Acceso Concedido";
                        AccessResultTextColor = Colors.Green;
                        AccessResultBorderColor = Colors.Green;
                    }
                    else
                    {
                        AccessResultIcon = "⛔";
                        AccessResultTitle = "Acceso Denegado";
                        AccessResultTextColor = Colors.Red;
                        AccessResultBorderColor = Colors.Red;
                    }
                    
                    AccessResultMessage = response.Message;
                    ShowAccessResult = true;
                    
                    _logger.LogInformation("✅ Card de resultado mostrada");
                    
                    // Ocultar la card después de 5 segundos
                    Task.Delay(5000).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ShowAccessResult = false;
                        });
                    });
                }
                catch (Exception cardEx)
                {
                    _logger.LogError(cardEx, "Error mostrando card de resultado");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling access response");
        }
    }

    private void OnConnectivityChanged(object? sender, Microsoft.Maui.Networking.ConnectivityChangedEventArgs e)
    {
        var wasOffline = !IsOnline;
        UpdateConnectivityStatus();
        
        // Si volvimos a estar online, sincronizar eventos pendientes
        if (wasOffline && IsOnline)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _syncService.SyncPendingEventsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing pending events after reconnection");
                }
            });
        }
    }

    private void UpdateConnectivityStatus()
    {
        IsOnline = Connectivity.NetworkAccess == NetworkAccess.Internet;
        
        if (IsOnline)
        {
            ConnectivityStatusText = "Online";
            ConnectivityStatusIcon = "✅";
            ConnectivityStatusColor = Colors.Green;
        }
        else
        {
            ConnectivityStatusText = "Offline";
            ConnectivityStatusIcon = "⚠️";
            ConnectivityStatusColor = Colors.Orange;
        }
    }

    private async Task ToggleEmulation()
    {
        if (IsEmulating)
        {
            StopEmulation();
        }
        else
        {
            await StartEmulation();
        }
    }

    private async Task LoadUserCredentials()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            
            if (user != null)
            {
                UserId = user.UserId;
                CredentialId = user.CredentialId;
                UserName = user.FullName;
                Roles = string.Join(", ", user.Roles);
                IsHceAvailable = _nfcCredentialService.IsHceAvailable;

                if (!CredentialId.HasValue)
                {
                    StatusMessage = "⚠️ Usuario sin credencial asignada";
                    _logger.LogWarning("User {UserId} has no credential assigned", UserId);
                }
                else
                {
                    StatusMessage = "Toca el botón para activar";
                    _logger.LogInformation("User credentials loaded: UserId={UserId}, CredentialId={CredentialId}", 
                        UserId, CredentialId);
                }
            }
            else
            {
                StatusMessage = "No hay usuario autenticado";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user credentials");
            StatusMessage = "Error cargando credencial";
        }
    }

    private async Task StartEmulation()
    {
        _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        _logger.LogInformation("🔄 StartEmulation CALLED");
        _logger.LogInformation("   UserId: {UserId}", UserId);
        _logger.LogInformation("   CredentialId: {CredentialId}", CredentialId);
        _logger.LogInformation("   UserId.HasValue: {HasValue}", UserId.HasValue);
        _logger.LogInformation("   CredentialId.HasValue: {HasValue}", CredentialId.HasValue);
        
        if (!UserId.HasValue || !CredentialId.HasValue)
        {
            _logger.LogError("❌ CANNOT START EMULATION - Missing values!");
            _logger.LogError("   UserId: {UserId} (HasValue: {HasValue})", UserId, UserId.HasValue);
            _logger.LogError("   CredentialId: {CredentialId} (HasValue: {HasValue})", CredentialId, CredentialId.HasValue);
            await _dialogService.ShowAlertAsync("Error", 
                $"No se pudo cargar la credencial.\n\nUserId: {UserId}\nCredentialId: {CredentialId}\n\nIntenta cerrar sesión e iniciar de nuevo.");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Iniciando emulación...";

            _logger.LogInformation("✅ Setting values in NFC service:");
            _logger.LogInformation("   UserId: {UserId}", UserId);
            _logger.LogInformation("   CredentialId: {CredentialId}", CredentialId);
            
            _nfcCredentialService.UserId = UserId;
            _nfcCredentialService.CredentialId = CredentialId;

            _logger.LogInformation("🚀 Calling StartEmulatingAsync...");
            await _nfcCredentialService.StartEmulatingAsync();

            IsEmulating = true;
            StatusMessage = "Credencial activa\nAcerca tu celular al punto de control";
            ActivationButtonText = "⏸️ Desactivar Credencial";
            ActivationButtonColor = Colors.Red;
            
            _logger.LogInformation("Credential emulation started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting credential emulation");
            await _dialogService.ShowAlertAsync("Error", $"No se pudo iniciar la emulación:\n{ex.Message}");
            StatusMessage = "❌ Error al iniciar emulación";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StopEmulation()
    {
        try
        {
            _nfcCredentialService.StopEmulating();
            IsEmulating = false;
            StatusMessage = "Toca el botón para activar";
            ActivationButtonText = "🚀 Activar Credencial";
            ActivationButtonColor = Colors.Green;
            _logger.LogInformation("Credential emulation stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping credential emulation");
        }
    }

    private async Task Logout()
    {
        try
        {
            StopEmulation(); // Stop any active emulation
            
            // Unsubscribe from events
            _nfcCredentialService.AccessResponseReceived -= OnAccessResponseReceived;
            
            await _authService.LogoutAsync();
            
            // Navegar a la página de login
            await _navigationService.NavigateToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            await _dialogService.ShowAlertAsync("Error", "Error al cerrar sesión");
        }
    }
}
