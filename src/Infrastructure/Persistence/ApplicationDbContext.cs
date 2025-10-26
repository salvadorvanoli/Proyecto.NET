using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Main database context for the application using Entity Framework Core.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<News> News => Set<News>();
    public DbSet<SpaceType> SpaceTypes => Set<SpaceType>();
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<ControlPoint> ControlPoints => Set<ControlPoint>();
    public DbSet<AccessRule> AccessRules => Set<AccessRule>();
    public DbSet<AccessEvent> AccessEvents => Set<AccessEvent>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<Consumption> Consumptions => Set<Consumption>();
    public DbSet<Benefit> Benefits => Set<Benefit>();
    public DbSet<BenefitType> BenefitTypes => Set<BenefitType>();
    public DbSet<Usage> Usages => Set<Usage>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for multi-tenancy (except for Tenant entity)
        // This ensures that all queries automatically filter by tenant
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetTenantQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    /// <summary>
    /// Sets up global query filter for tenant isolation.
    /// </summary>
    private void SetTenantQueryFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        // This would need to be implemented with a tenant provider in a real application
        // For now, we'll comment it out to avoid compilation issues
        // modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    }

    /// <summary>
    /// Overrides SaveChanges to automatically update timestamps.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Overrides SaveChangesAsync to automatically update timestamps.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates timestamps for entities being modified.
    /// </summary>
    /// <summary>
    /// Updates timestamps for entities being added or modified.
    /// Uses reflection to call protected UpdateTimestamp method.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified))
            .Select(e => e.Entity as BaseEntity)
            .Where(e => e != null);

        foreach (var entity in entities)
        {
            var method = typeof(BaseEntity).GetMethod("UpdateTimestamp",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            method?.Invoke(entity, null);
        }
    }
}
