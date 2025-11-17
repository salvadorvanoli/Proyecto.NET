namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Service interface for tenant operations.
/// </summary>
public interface ITenantApiService
{
    /// <summary>
    /// Gets the theme configuration for a specific tenant.
    /// </summary>
    Task<TenantThemeDto?> GetTenantThemeAsync(int tenantId);
}

/// <summary>
/// DTO for tenant theme configuration.
/// </summary>
public class TenantThemeDto
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string AccentColor { get; set; } = string.Empty;
    public string? Logo { get; set; }
}
