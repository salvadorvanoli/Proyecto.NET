using Application.Common.Interfaces;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.Services.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add HTTP Context Accessor for tenant resolution
        services.AddHttpContextAccessor();

        // Add Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);

            // Suppress pending model changes warning - interface changes don't affect database schema
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            // Enable sensitive data logging in development
            #if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            #endif
        });

        // Register ApplicationDbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register infrastructure services
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<Application.Benefits.Services.IBenefitService, Services.Benefits.BenefitService>();
        services.AddScoped<Application.AccessEvents.Services.IAccessEventService, Services.AccessEvents.AccessEventService>();
        // Token service for JWT generation
        services.AddSingleton<ITokenService, TokenService>();

        services.AddScoped<INotificationHubService, NotificationHubService>();

        // Add Redis caching
        AddRedisCaching(services, configuration);

        return services;
    }

    /// <summary>
    /// Adds Redis caching services to the dependency injection container.
    /// </summary>
    private static void AddRedisCaching(IServiceCollection services, IConfiguration configuration)
    {
        // Configure cache options
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        var cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>();
        
        if (cacheOptions?.Enabled == true)
        {
            var redisConnection = configuration.GetConnectionString("Redis");

            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                // Register IConnectionMultiplexer for direct Redis access (needed for pattern matching)
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var configurationOptions = ConfigurationOptions.Parse(redisConnection);
                    return ConnectionMultiplexer.Connect(configurationOptions);
                });

                // Register the enhanced cache service with pattern support
                services.AddSingleton<ICacheService, RedisEnhancedCacheService>();

                // Also register IDistributedCache for compatibility
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "ProyectoNet:";
                });
            }
            else
            {
                // Fallback to in-memory cache if Redis is not configured
                services.AddDistributedMemoryCache();
                services.AddSingleton<ICacheService, RedisCacheService>();
            }
        }
        else
        {
            // If caching is disabled, use a no-op implementation
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        // Register cache metrics service (singleton to maintain metrics across requests)
        services.AddSingleton<ICacheMetricsService, CacheMetricsService>();
    }
}

