using Application.Common.Interfaces;
using Domain.DataTypes;
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

        // Subscribe to StateChanged event to reconstruct ValidityPeriod when entities are loaded
        ChangeTracker.StateChanged += OnEntityStateChanged;
        ChangeTracker.Tracked += OnEntityTracked;
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
    ///   * During seeding/migrations, tenant filter is bypassed because HttpContext is unavailable
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
            _tenantProvider != null &&
            _tenantProvider.IsHttpContextAvailable() &&
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
    /// Also synchronizes shadow properties for ValidityPeriod.
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

        // Synchronize ValidityPeriod shadow properties for Benefit entities
        var benefitEntries = ChangeTracker.Entries<Benefit>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in benefitEntries)
        {
            var benefit = entry.Entity;
            if (benefit.ValidityPeriod.HasValue)
            {
                entry.Property("ValidityStartDate").CurrentValue = benefit.ValidityPeriod.Value.StartDate;
                entry.Property("ValidityEndDate").CurrentValue = benefit.ValidityPeriod.Value.EndDate;
            }
            else
            {
                entry.Property("ValidityStartDate").CurrentValue = null;
                entry.Property("ValidityEndDate").CurrentValue = null;
            }
        }

        // Synchronize ValidityPeriod and TimeRange shadow properties for AccessRule entities
        var accessRuleEntries = ChangeTracker.Entries<AccessRule>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in accessRuleEntries)
        {
            var accessRule = entry.Entity;
            
            // Synchronize ValidityPeriod
            if (accessRule.ValidityPeriod.HasValue)
            {
                entry.Property("ValidityStartDate").CurrentValue = accessRule.ValidityPeriod.Value.StartDate;
                entry.Property("ValidityEndDate").CurrentValue = accessRule.ValidityPeriod.Value.EndDate;
            }
            else
            {
                entry.Property("ValidityStartDate").CurrentValue = null;
                entry.Property("ValidityEndDate").CurrentValue = null;
            }
            
            // Synchronize TimeRange
            if (accessRule.TimeRange.HasValue)
            {
                entry.Property("TimeRangeStartTime").CurrentValue = accessRule.TimeRange.Value.StartTime;
                entry.Property("TimeRangeEndTime").CurrentValue = accessRule.TimeRange.Value.EndTime;
            }
            else
            {
                entry.Property("TimeRangeStartTime").CurrentValue = null;
                entry.Property("TimeRangeEndTime").CurrentValue = null;
            }
        }
    }

    /// <summary>
    /// Event handler when entity is first tracked (loaded from database).
    /// </summary>
    private void OnEntityTracked(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs e)
    {
        ReconstructValidityPeriod(e.Entry);
    }

    /// <summary>
    /// Event handler when entity state changes.
    /// </summary>
    private void OnEntityStateChanged(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs e)
    {
        if (e.NewState == EntityState.Unchanged || e.NewState == EntityState.Modified)
        {
            ReconstructValidityPeriod(e.Entry);
        }
    }

    /// <summary>
    /// Reconstructs ValidityPeriod from shadow properties when loading entities.
    /// </summary>
    private void ReconstructValidityPeriod(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.Entity is Benefit benefit)
        {
            var startDate = entry.Property("ValidityStartDate").CurrentValue as DateOnly?;
            var endDate = entry.Property("ValidityEndDate").CurrentValue as DateOnly?;

            if (startDate.HasValue && endDate.HasValue)
            {
                var validityPeriodProperty = typeof(Benefit).GetProperty("ValidityPeriod");
                validityPeriodProperty?.SetValue(benefit, new DateRange(startDate.Value, endDate.Value));
            }
        }
        else if (entry.Entity is AccessRule accessRule)
        {
            var startDate = entry.Property("ValidityStartDate").CurrentValue as DateOnly?;
            var endDate = entry.Property("ValidityEndDate").CurrentValue as DateOnly?;
            var startTime = entry.Property("TimeRangeStartTime").CurrentValue as TimeOnly?;
            var endTime = entry.Property("TimeRangeEndTime").CurrentValue as TimeOnly?;

            if (startDate.HasValue && endDate.HasValue)
            {
                var validityPeriodProperty = typeof(AccessRule).GetProperty("ValidityPeriod");
                validityPeriodProperty?.SetValue(accessRule, new DateRange(startDate.Value, endDate.Value));
            }
            
            if (startTime.HasValue && endTime.HasValue)
            {
                var timeRangeProperty = typeof(AccessRule).GetProperty("TimeRange");
                timeRangeProperty?.SetValue(accessRule, new TimeRange(startTime.Value, endTime.Value));
            }
        }
    }

    /// <summary>
    /// Public method to hydrate AccessRule properties from shadow properties.
    /// Used by Application layer when needed.
    /// </summary>
    public void HydrateAccessRuleProperties(AccessRule accessRule)
    {
        var entry = Entry(accessRule);
        ReconstructValidityPeriod(entry);
    }
}
