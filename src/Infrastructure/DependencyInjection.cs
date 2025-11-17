using Application.Common.Interfaces;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Register database seeder
        services.AddScoped<DbSeeder>();
        services.AddScoped<INotificationHubService, NotificationHubService>();

        return services;
    }
}
