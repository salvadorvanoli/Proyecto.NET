using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Main database context for the application using Entity Framework Core.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantProvider? _tenantProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider? tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
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

        // ========================================
        // MULTI-TENANCY: Global Query Filters
        // ========================================
        // Automatically filters ALL queries by TenantId to ensure data isolation
        // between tenants. This provides an additional security layer on top of
        // manual filtering in services.
        //
        // HOW IT WORKS:
        // - During runtime (HTTP requests): ITenantProvider extracts tenant from JWT
        // - During migrations/seeds: ITenantProvider is null, so filters are skipped
        // - To bypass filters in code: use .IgnoreQueryFilters() in LINQ queries
        //
        // SECURITY BENEFITS:
        // - Prevents accidental cross-tenant data leaks
        // - Protects against forgotten .Where(x => x.TenantId == ...) filters
        // - Works transparently with Include(), joins, and complex queries
        // ========================================
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
    /// IMPORTANT: This filter checks if HTTP context is available before applying tenant filter.
    /// 
    /// SECURITY CONSIDERATIONS:
    /// - During HTTP requests: Filter ALWAYS applies (IsHttpContextAvailable() = true)
    /// - During migrations/seeds: Filter disabled (no HttpContext), which is SAFE because:
    ///   * Migrations run in controlled environment (dev/CI/CD)
    ///   * Seeders explicitly use .IgnoreQueryFilters() for cross-tenant setup
    ///   * Production seeds run during deployment, not runtime
    /// 
    /// If HttpContext becomes unavailable during runtime (extremely rare edge case),
    /// the request would fail at authentication/authorization before reaching queries.
    /// </summary>
    private void SetTenantQueryFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        // Apply filter that checks HTTP context availability at query time
        // This prevents crashes during migrations and seeds when HttpContext is not available
        // but ITenantProvider is still injected by DI
        modelBuilder.Entity<T>().HasQueryFilter(e => 
            _tenantProvider == null || 
            !_tenantProvider.IsHttpContextAvailable() ||
            e.TenantId == _tenantProvider.GetCurrentTenantId());
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
