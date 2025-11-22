using Application.Common.DTOs;

namespace Application.Common.Interfaces;

/// <summary>
/// Service for tenant operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the theme configuration for a specific tenant.
    /// </summary>
    Task<TenantThemeDto?> GetTenantThemeAsync(int tenantId, CancellationToken cancellationToken = default);
}
