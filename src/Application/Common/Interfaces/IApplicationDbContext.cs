using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

/// <summary>
/// Interface for the application database context.
/// Allows Application layer to access data without depending on Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Credential> Credentials { get; }
    DbSet<Role> Roles { get; }
    DbSet<Space> Spaces { get; }
    DbSet<SpaceType> SpaceTypes { get; }
    DbSet<ControlPoint> ControlPoints { get; }
    DbSet<AccessRule> AccessRules { get; }
    DbSet<AccessEvent> AccessEvents { get; }
    DbSet<News> News { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Consumption> Consumptions { get; }
    DbSet<Benefit> Benefits { get; }
    DbSet<BenefitType> BenefitTypes { get; }
    DbSet<Usage> Usages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
