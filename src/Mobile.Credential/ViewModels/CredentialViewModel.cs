using System.Windows.Input;
using Mobile.Credential.Services;
using Microsoft.Extensions.Logging;

namespace Mobile.Credential.ViewModels;

/// <summary>
/// ViewModel for the digital credential view
/// Handles NFC credential emulation (HCE mode)
/// </summary>
public class CredentialViewModel : BaseViewModel
{
    private readonly INfcCredentialService _nfcCredentialService;
    private readonly IAuthService _authService;
    private readonly ILogger<CredentialViewModel> _logger;

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
    
    // Access Response properties
    private bool _showAccessResponse;
    private string _accessResponseIcon = string.Empty;
    private string _accessResponseTitle = string.Empty;
    private string _accessResponseMessage = string.Empty;
    private Color _accessResponseBackgroundColor = Colors.Gray;
    private Color _accessResponseBorderColor = Colors.Gray;

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

    public bool ShowAccessResponse
    {
        get => _showAccessResponse;
        set => SetProperty(ref _showAccessResponse, value);
    }

    public string AccessResponseIcon
    {
        get => _accessResponseIcon;
        set => SetProperty(ref _accessResponseIcon, value);
    }

    public string AccessResponseTitle
    {
        get => _accessResponseTitle;
        set => SetProperty(ref _accessResponseTitle, value);
    }

    public string AccessResponseMessage
    {
        get => _accessResponseMessage;
        set => SetProperty(ref _accessResponseMessage, value);
    }

    public Color AccessResponseBackgroundColor
    {
        get => _accessResponseBackgroundColor;
        set => SetProperty(ref _accessResponseBackgroundColor, value);
    }

    public Color AccessResponseBorderColor
    {
        get => _accessResponseBorderColor;
        set => SetProperty(ref _accessResponseBorderColor, value);
    }

    public ICommand StartEmulationCommand { get; }
    public ICommand StopEmulationCommand { get; }
    public ICommand ToggleEmulationCommand { get; }
    public ICommand LogoutCommand { get; }

    public CredentialViewModel(
        INfcCredentialService nfcCredentialService,
        IAuthService authService,
        ILogger<CredentialViewModel> logger)
    {
        _nfcCredentialService = nfcCredentialService;
        _authService = authService;
        _logger = logger;

        Title = "Mi Credencial Digital";

        StartEmulationCommand = new Command(async () => await StartEmulation());
        StopEmulationCommand = new Command(StopEmulation);
        ToggleEmulationCommand = new Command(async () => await ToggleEmulation());
        LogoutCommand = new Command(async () => await Logout());
        
        // Suscribirse al evento de respuesta de acceso
        _nfcCredentialService.AccessResponseReceived += OnAccessResponseReceived;
        
        // Subscribe to connectivity changes
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        UpdateConnectivityStatus();
        
        // Load user credentials automatically
        Task.Run(async () => await LoadUserCredentials());
    }

    private void OnConnectivityChanged(object? sender, Microsoft.Maui.Networking.ConnectivityChangedEventArgs e)
    {
        UpdateConnectivityStatus();
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
                CredentialId = user.UserId; // For now, credential ID = user ID
                UserName = user.FullName;
                Roles = string.Join(", ", user.Roles);
                IsHceAvailable = _nfcCredentialService.IsHceAvailable;

                StatusMessage = "Toca el botón para activar";
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
        if (!UserId.HasValue || !CredentialId.HasValue)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo cargar la credencial. Intenta cerrar sesión e iniciar de nuevo.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Iniciando emulación...";

            _nfcCredentialService.UserId = UserId;
            _nfcCredentialService.CredentialId = CredentialId;

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
            await Shell.Current.DisplayAlert("Error", $"No se pudo iniciar la emulación:\n{ex.Message}", "OK");
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
            
            await _authService.LogoutAsync();
            
            // Navegar a la página de login
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            await Shell.Current.DisplayAlert("Error", "Error al cerrar sesión", "OK");
        }
    }

    private async void OnAccessResponseReceived(object? sender, AccessResponseEventArgs e)
    {
        _logger.LogInformation("👀 Access response received in ViewModel: {Type} - {Message}",
            e.IsGranted ? "GRANTED" : "DENIED", e.Message);

        // Asegurar que se ejecute en el thread de UI
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                ShowAccessResponse = true;
                
                if (e.IsGranted)
                {
                    // Acceso concedido - Verde
                    AccessResponseIcon = "✅";
                    AccessResponseTitle = "ACCESO PERMITIDO";
                    AccessResponseMessage = e.Message;
                    AccessResponseBackgroundColor = Colors.Green;
                    AccessResponseBorderColor = Colors.DarkGreen;
                    
                    _logger.LogInformation("🟢 Showing ACCESS GRANTED UI");
                }
                else
                {
                    // Acceso denegado - Rojo
                    AccessResponseIcon = "❌";
                    AccessResponseTitle = "ACCESO DENEGADO";
                    AccessResponseMessage = e.Message;
                    AccessResponseBackgroundColor = Colors.Red;
                    AccessResponseBorderColor = Colors.DarkRed;
                    
                    _logger.LogInformation("🔴 Showing ACCESS DENIED UI");
                }
                
                // Ocultar después de 5 segundos
                await Task.Delay(5000);
                ShowAccessResponse = false;
                
                _logger.LogInformation("🔲 Access response hidden after 5 seconds");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing access response");
            }
        });
    }
}

