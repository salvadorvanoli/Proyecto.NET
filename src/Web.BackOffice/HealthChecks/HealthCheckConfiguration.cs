using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Web.BackOffice.HealthChecks;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddBackOfficeHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddSqlServer(
                connectionString,
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "sqlserver" });
        }

        var apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        if (!string.IsNullOrEmpty(apiBaseUrl))
        {
            healthChecksBuilder.AddUrlGroup(
                new Uri($"{apiBaseUrl}/health"),
                name: "api",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "api", "external" });
        }

        return services;
    }

    public static IEndpointRouteBuilder MapBackOfficeHealthChecks(this IEndpointRouteBuilder endpoints)
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
        }).AllowAnonymous(); // Permitir acceso sin autenticación

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous(); // Permitir acceso sin autenticación

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("api")
        }).AllowAnonymous(); // Permitir acceso sin autenticación

        return endpoints;
    }
}
