namespace Application.Common.DTOs;

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
