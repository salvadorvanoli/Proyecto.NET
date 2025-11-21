namespace Mobile.Models;

/// <summary>
/// Datos seguros de sesión que se guardan en SecureStorage
/// NO incluye información sensible como contraseñas
/// </summary>
public class SecureSession
{
    public int UserId { get; set; }
    public int? CredentialId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public List<string> Roles { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    
    /// <summary>
    /// Verifica si el token ha expirado
    /// </summary>
    public bool IsTokenExpired()
    {
        return DateTime.UtcNow >= ExpiresAtUtc;
    }
    
    /// <summary>
    /// Verifica si el token está próximo a expirar (menos de 5 minutos)
    /// </summary>
    public bool IsTokenExpiringSoon()
    {
        return DateTime.UtcNow >= ExpiresAtUtc.AddMinutes(-5);
    }
}
