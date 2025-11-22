using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Web.FrontOffice.HealthChecks;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddFrontOfficeHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        var apiBaseUrl = configuration["API_BASE_URL"] ?? Environment.GetEnvironmentVariable("API_BASE_URL");
        if (!string.IsNullOrEmpty(apiBaseUrl))
        {
            // Normalizar la URL base (quitar /api si ya está presente)
            apiBaseUrl = apiBaseUrl.TrimEnd('/');
            if (apiBaseUrl.EndsWith("/api"))
            {
                apiBaseUrl = apiBaseUrl.Substring(0, apiBaseUrl.Length - 4);
            }

            // Construir la URL del health check
            var apiHealthUrl = apiBaseUrl + "/api/health";

            healthChecksBuilder.AddUrlGroup(
                new Uri(apiHealthUrl),
                name: "api",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "api", "external" });
        }

        return services;
    }

    public static IEndpointRouteBuilder MapFrontOfficeHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.TotalMilliseconds
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                });
                await context.Response.WriteAsync(result);
            }
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("api")
        });

        return endpoints;
    }
}
