using Mobile.Services;
using Mobile.Validators;
using System.Windows.Input;

namespace Mobile.ViewModels;

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

        // Validar credenciales con validador robusto
        var validation = LoginValidator.ValidateCredentials(Username, Password);
        if (!validation.IsValid)
        {
            ErrorMessage = validation.ErrorMessage!;
            HasError = true;
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _authService.LoginAsync(Username, Password);

            if (result != null)
            {
                // Login exitoso - navegar a CredentialPage
                await Shell.Current.GoToAsync("//CredentialPage");
            }
            else
            {
                ErrorMessage = "Usuario o contraseña incorrectos";
                HasError = true;
            }
        }
        catch (InvalidOperationException ex)
        {
            // Errores controlados del servicio (red, etc.)
            ErrorMessage = ex.Message;
            HasError = true;
        }
        catch (Exception ex)
        {
            // Errores inesperados
            System.Diagnostics.Debug.WriteLine($"Error inesperado en LoginAsync: {ex}");
            ErrorMessage = "Ocurrió un error inesperado. Por favor, intenta nuevamente.";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
