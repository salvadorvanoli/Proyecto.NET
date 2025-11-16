using System.Windows.Input;
using Mobile.Services;
using Mobile.Pages;
using Microsoft.Extensions.Logging;

namespace Mobile.ViewModels;

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

    public ICommand StartEmulationCommand { get; }
    public ICommand StopEmulationCommand { get; }
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
        LogoutCommand = new Command(async () => await Logout());
        
        // Load user credentials automatically
        Task.Run(async () => await LoadUserCredentials());
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

                StatusMessage = $"Credencial lista\n{UserName}\nRoles: {Roles}";
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
            StatusMessage = "✅ Credencial activa\n\nAcerca tu celular al punto de control";
            
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
            StatusMessage = "Emulación detenida";
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
            
            // Cambiar a la página de login
            var loginViewModel = new LoginViewModel(_authService);
            var loginPage = new LoginPage(loginViewModel);
            Microsoft.Maui.Controls.Application.Current!.MainPage = new NavigationPage(loginPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            
            // Usar Application.Current.MainPage para mostrar el alert
            if (Microsoft.Maui.Controls.Application.Current?.MainPage != null)
            {
                await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                    "Error", "Error al cerrar sesión", "OK");
            }
        }
    }
}
