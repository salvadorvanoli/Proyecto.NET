using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

namespace Web.Api.Configuration;

/// <summary>
/// Configuración de OpenTelemetry para métricas y trazas distribuidas
/// </summary>
public static class OpenTelemetryConfiguration
{
    // Métricas personalizadas de la aplicación
    public static readonly Meter AppMeter = new("ProyectoNet.API", "1.0.0");

    // Contadores de métricas
    public static readonly Counter<long> RequestCounter = AppMeter.CreateCounter<long>(
        "api.requests.total",
        description: "Número total de peticiones HTTP");

    public static readonly Counter<long> ErrorCounter = AppMeter.CreateCounter<long>(
        "api.errors.total",
        description: "Número total de errores");

    public static readonly Histogram<double> RequestDuration = AppMeter.CreateHistogram<double>(
        "api.request.duration",
        unit: "ms",
        description: "Duración de las peticiones HTTP en milisegundos");

    public static void ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        var serviceName = "ProyectoNet.API";
        var serviceVersion = "1.0.0";

        builder.Services.AddOpenTelemetry()
            // Configurar información del servicio
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName
                }))

            // Configurar trazas (traces)
            .WithTracing(tracing => tracing
                // Instrumentación automática de ASP.NET Core
                .AddAspNetCoreInstrumentation(options =>
                {
                    // Registrar detalles de las peticiones
                    options.RecordException = true;
                })

                // Instrumentación de peticiones HTTP salientes
                .AddHttpClientInstrumentation()

                // Instrumentación de SQL Server
                .AddSqlClientInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.RecordException = true;
                })

                // Exportar a consola (para desarrollo)
                .AddConsoleExporter())

            // Configurar métricas
            .WithMetrics(metrics => metrics
                // Métricas de ASP.NET Core
                .AddAspNetCoreInstrumentation()

                // Métricas de HTTP Client
                .AddHttpClientInstrumentation()

                // Métricas personalizadas de la aplicación
                .AddMeter(AppMeter.Name)

                // Exportar a consola (para desarrollo)
                .AddConsoleExporter());

        builder.Services.AddSingleton(AppMeter);
    }
}
