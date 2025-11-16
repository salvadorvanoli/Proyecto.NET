using System.Windows.Input;
using Mobile.Models;
using Mobile.Services;

namespace Mobile.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private AppMode _selectedMode;
    private string _modeDescription = string.Empty;

    public AppMode SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (SetProperty(ref _selectedMode, value))
            {
                UpdateModeDescription();
            }
        }
    }

    public string ModeDescription
    {
        get => _modeDescription;
        set => SetProperty(ref _modeDescription, value);
    }

    public ICommand SaveAndNavigateCommand { get; }

    public SettingsViewModel()
    {
        Title = "Configuración";
        SelectedMode = AppSettings.CurrentMode;
        
        SaveAndNavigateCommand = new Command(async () => await SaveAndNavigate());
        
        UpdateModeDescription();
    }

    private void UpdateModeDescription()
    {
        ModeDescription = SelectedMode switch
        {
            AppMode.Credential => "📱 Modo Credencial\n\nTu celular emulará una credencial NFC. Acércalo a un punto de control para validar tu acceso.",
            AppMode.ControlPoint => "🚪 Modo Punto de Control\n\nTu celular actuará como punto de control. Leerá credenciales NFC de otros dispositivos.",
            _ => "Selecciona un modo"
        };
    }

    private async Task SaveAndNavigate()
    {
        AppSettings.CurrentMode = SelectedMode;

        await Shell.Current.DisplayAlert(
            "Modo Guardado",
            $"La aplicación ahora está en modo: {(SelectedMode == AppMode.Credential ? "Credencial" : "Punto de Control")}",
            "OK");

        // Navigate to the appropriate page
        if (SelectedMode == AppMode.Credential)
        {
            await Shell.Current.GoToAsync("//CredentialPage");
        }
        else
        {
            await Shell.Current.GoToAsync("//AccessNfcPage");
        }
    }
}
