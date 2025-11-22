using Mobile.AccessPoint.Services;
using System.Windows.Input;

namespace Mobile.AccessPoint.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginCommand = new Command(async () => await LoginAsync());
    }

    private async Task LoginAsync()
    {
        if (IsBusy) return;

        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Usuario y contraseña son requeridos";
            HasError = true;
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _authService.LoginAsync(Username, Password);

            if (result != null)
            {
                // Login exitoso - navegar a AppShell
                Microsoft.Maui.Controls.Application.Current!.MainPage = new AppShell();
            }
            else
            {
                ErrorMessage = "Usuario o contraseña incorrectos";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

