using Application.Auth;
using Application.Common.Interfaces;
using Application.Users;
using Application.Roles;
using Application.News;
using Application.SpaceTypes;
using Application.Spaces;
using Application.ControlPoints;
using Application.AccessRules;
using Application.AccessEvents;
using Application.BenefitTypes;
using Application.Benefits;
using Application.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Extension methods for registering application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<ISpaceTypeService, SpaceTypeService>();
        services.AddScoped<ISpaceService, SpaceService>();
        services.AddScoped<IControlPointService, ControlPointService>();
        services.AddScoped<IAccessValidationService, AccessValidationService>();
        services.AddScoped<IBenefitTypeService, BenefitTypeService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Register services with caching decorators
        // The decorator pattern allows us to add caching behavior without modifying the original services
        services.AddScoped<BenefitService>(); // Register the concrete implementation
        services.AddScoped<IBenefitService>(provider =>
        {
            var innerService = provider.GetRequiredService<BenefitService>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            var tenantProvider = provider.GetRequiredService<ITenantProvider>();
            return new CachedBenefitService(innerService, cacheService, tenantProvider);
        });

        services.AddScoped<AccessRuleService>(); // Register the concrete implementation
        services.AddScoped<IAccessRuleService>(provider =>
        {
            var innerService = provider.GetRequiredService<AccessRuleService>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            var tenantProvider = provider.GetRequiredService<ITenantProvider>();
            return new CachedAccessRuleService(innerService, cacheService, tenantProvider);
        });

        return services;
    }
}
