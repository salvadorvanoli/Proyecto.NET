using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of tenant service.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IApplicationDbContext _context;

    public TenantService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantThemeDto?> GetTenantThemeAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .Where(t => t.Id == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant == null)
        {
            return null;
        }

        return new TenantThemeDto
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            AccentColor = tenant.AccentColor,
            Logo = tenant.Logo
        };
    }
}
