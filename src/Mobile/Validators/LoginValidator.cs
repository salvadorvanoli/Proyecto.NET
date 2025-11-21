using System.Text.RegularExpressions;

namespace Mobile.Validators;

/// <summary>
/// Validador para credenciales de login
/// </summary>
public static class LoginValidator
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Valida el email
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "El email es requerido");

        if (email.Length > 254)
            return (false, "El email es demasiado largo");

        if (!EmailRegex.IsMatch(email))
            return (false, "El formato del email no es válido");

        return (true, null);
    }

    /// <summary>
    /// Valida la contraseña
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña es requerida");

        if (password.Length < 6)
            return (false, "La contraseña debe tener al menos 6 caracteres");

        if (password.Length > 100)
            return (false, "La contraseña es demasiado larga");

        return (true, null);
    }

    /// <summary>
    /// Valida ambos email y contraseña
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidateCredentials(string email, string password)
    {
        var emailValidation = ValidateEmail(email);
        if (!emailValidation.IsValid)
            return emailValidation;

        var passwordValidation = ValidatePassword(password);
        if (!passwordValidation.IsValid)
            return passwordValidation;

        return (true, null);
    }
}
