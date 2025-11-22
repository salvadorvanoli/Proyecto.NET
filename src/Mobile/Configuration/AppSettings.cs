namespace Mobile.Configuration;

/// <summary>
/// Configuración de la API obtenida desde appsettings.json
/// NO incluir secretos aquí - usar SecureStorage para datos sensibles
/// </summary>
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// Configuración de seguridad
/// </summary>
public class SecuritySettings
{
    public CertificatePinningSettings CertificatePinning { get; set; } = new();
}

public class CertificatePinningSettings
{
    public bool Enabled { get; set; }
    public List<string> Pins { get; set; } = new();
}

public class AppSettings
{
    public ApiSettings ApiSettings { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
}
