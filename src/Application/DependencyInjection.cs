using Application.Auth;
using Application.Common.Interfaces;
using Application.Users;
using Application.Roles;
using Application.News;
using Application.SpaceTypes;
using Application.Spaces;
using Application.ControlPoints;
using Application.AccessRules;
using Application.BenefitTypes;
using Application.Benefits;
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
        services.AddScoped<IAccessRuleService, AccessRuleService>();
        services.AddScoped<IBenefitTypeService, BenefitTypeService>();
        services.AddScoped<BenefitService>();

        return services;
    }
}
