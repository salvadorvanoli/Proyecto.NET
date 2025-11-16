using System.Windows.Input;
using Mobile.Services;
using Microsoft.Extensions.Logging;

namespace Mobile.ViewModels;

/// <summary>
/// ViewModel for the digital credential view
/// Handles NFC credential emulation (HCE mode)
/// </summary>
public class CredentialViewModel : BaseViewModel
{
    private readonly INfcCredentialService _nfcCredentialService;
    private readonly ILogger<CredentialViewModel> _logger;

    private int? _userId;
    private int? _credentialId;
    private string _userName = "Usuario";
    private bool _isEmulating;
    private bool _isHceAvailable;
    private string _statusMessage = "Configura tu credencial para comenzar";

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
    public ICommand SaveCredentialsCommand { get; }
    public ICommand GoToSettingsCommand { get; }

    public CredentialViewModel(
        INfcCredentialService nfcCredentialService,
        ILogger<CredentialViewModel> logger)
    {
        _nfcCredentialService = nfcCredentialService;
        _logger = logger;

        Title = "Mi Credencial Digital";

        StartEmulationCommand = new Command(async () => await StartEmulation());
        StopEmulationCommand = new Command(StopEmulation);
        SaveCredentialsCommand = new Command(async () => await SaveCredentials());
        GoToSettingsCommand = new Command(async () => await GoToSettings());
        
        // Load saved credentials
        LoadCredentials();
    }

    private void LoadCredentials()
    {
        UserId = AppSettings.UserId;
        CredentialId = AppSettings.CredentialId;
        IsHceAvailable = _nfcCredentialService.IsHceAvailable;

        if (UserId.HasValue && CredentialId.HasValue)
        {
            StatusMessage = $"Credencial configurada\nUsuario ID: {UserId}\nCredencial ID: {CredentialId}";
        }
    }

    private async Task StartEmulation()
    {
        if (!UserId.HasValue || !CredentialId.HasValue)
        {
            await Shell.Current.DisplayAlert("Error", "Debes configurar tu Usuario ID y Credencial ID primero", "OK");
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

    private async Task SaveCredentials()
    {
        if (!UserId.HasValue || !CredentialId.HasValue)
        {
            await Shell.Current.DisplayAlert("Error", "Debes ingresar Usuario ID y Credencial ID", "OK");
            return;
        }

        AppSettings.UserId = UserId;
        AppSettings.CredentialId = CredentialId;

        await Shell.Current.DisplayAlert("Guardado", "Credenciales guardadas correctamente", "OK");
        
        StatusMessage = $"Credencial configurada\nUsuario ID: {UserId}\nCredencial ID: {CredentialId}";
    }

    private async Task GoToSettings()
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }
}
