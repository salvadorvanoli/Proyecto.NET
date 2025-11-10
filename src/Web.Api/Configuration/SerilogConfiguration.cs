using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Web.Api.Configuration;

/// <summary>
/// Configuración de Serilog para logging estructurado
/// </summary>
public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            // Nivel mínimo de log (Debug en dev, Information en prod)
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)

            // Enriquecedores: agregan información adicional a cada log
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "ProyectoNet.API")

            // Sink: Consola (para desarrollo local y CloudWatch en AWS)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")

            // Sink: Archivo JSON estructurado (para análisis posterior)
            .WriteTo.File(
                new CompactJsonFormatter(),
                path: "logs/api-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)

            // Sink: Archivo de texto legible para desarrollo
            .WriteTo.File(
                path: "logs/api-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}")

            .CreateLogger();

        // Configurar Serilog como el logger de ASP.NET Core
        builder.Host.UseSerilog();

        Log.Information("🚀 Serilog configurado correctamente para {ApplicationName}", "ProyectoNet.API");
    }
}

