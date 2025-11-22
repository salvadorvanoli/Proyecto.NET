namespace Mobile.Services;

public interface IBiometricAuthService
{
    Task<bool> AuthenticateAsync(string title = "Autenticación requerida", string description = "Verifica tu identidad para continuar");
}
