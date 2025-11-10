using System.Diagnostics;

namespace Web.Api.Middleware;

/// <summary>
/// Middleware que agrega un CorrelationId único a cada petición HTTP
/// para trazabilidad end-to-end en logs y métricas
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener CorrelationId del header o generar uno nuevo
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Activity.Current?.Id
            ?? Guid.NewGuid().ToString();

        // Agregar CorrelationId al contexto para que esté disponible en toda la petición
        context.Items["CorrelationId"] = correlationId;

        // Agregar CorrelationId al Activity (para OpenTelemetry)
        Activity.Current?.SetTag("correlation_id", correlationId);

        // Agregar CorrelationId a la respuesta para que el cliente lo vea
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Add(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        // Continuar con el siguiente middleware
        await _next(context);
    }
}

/// <summary>
/// Extension method para agregar el middleware de CorrelationId
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}

