using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Api.Configuration;
using System.Diagnostics;

namespace Web.Api.Controllers;

/// <summary>
/// Controlador de ejemplo que muestra cómo usar métricas, logs y trazas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdministradorBackoffice")]
public class ObservabilityController : ControllerBase
{
    private readonly ILogger<ObservabilityController> _logger;

    public ObservabilityController(ILogger<ObservabilityController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Endpoint de ejemplo que registra métricas
    /// </summary>
    [HttpGet("metrics-example")]
    public IActionResult MetricsExample()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Obtener CorrelationId del contexto
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";

            // Log estructurado con CorrelationId
            _logger.LogInformation(
                "Processing metrics example | CorrelationId: {CorrelationId}",
                correlationId);

            // Simular procesamiento
            Thread.Sleep(Random.Shared.Next(10, 100));

            // Registrar métricas
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalMilliseconds;

            // Incrementar contador de peticiones
            OpenTelemetryConfiguration.RequestCounter.Add(1,
                new KeyValuePair<string, object?>("endpoint", "metrics-example"),
                new KeyValuePair<string, object?>("method", "GET"),
                new KeyValuePair<string, object?>("status", 200));

            // Registrar duración de la petición
            OpenTelemetryConfiguration.RequestDuration.Record(duration,
                new KeyValuePair<string, object?>("endpoint", "metrics-example"),
                new KeyValuePair<string, object?>("method", "GET"));

            return Ok(new
            {
                message = "Métricas registradas correctamente",
                correlationId,
                duration = $"{duration:F2}ms",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // Registrar error
            OpenTelemetryConfiguration.ErrorCounter.Add(1,
                new KeyValuePair<string, object?>("endpoint", "metrics-example"),
                new KeyValuePair<string, object?>("error_type", ex.GetType().Name));

            _logger.LogError(ex, "Error en metrics-example");
            throw;
        }
    }

    /// <summary>
    /// Endpoint que simula diferentes latencias para demostrar percentiles
    /// </summary>
    [HttpGet("latency-test")]
    public async Task<IActionResult> LatencyTest([FromQuery] int delayMs = 0)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";

        _logger.LogInformation("Iniciando latency test con delay de {DelayMs}ms | CorrelationId: {CorrelationId}",
            delayMs, correlationId);

        // Simular procesamiento con diferentes latencias
        if (delayMs > 0)
        {
            await Task.Delay(delayMs);
        }
        else
        {
            // Simular latencia aleatoria para generar percentiles
            var randomDelay = Random.Shared.Next(5, 500);
            await Task.Delay(randomDelay);
        }

        stopwatch.Stop();
        var duration = stopwatch.Elapsed.TotalMilliseconds;

        // Registrar métricas
        OpenTelemetryConfiguration.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", "latency-test"),
            new KeyValuePair<string, object?>("method", "GET"));

        OpenTelemetryConfiguration.RequestDuration.Record(duration,
            new KeyValuePair<string, object?>("endpoint", "latency-test"));

        _logger.LogInformation("Latency test completado en {Duration}ms | CorrelationId: {CorrelationId}",
            duration, correlationId);

        return Ok(new
        {
            correlationId,
            duration = $"{duration:F2}ms",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Endpoint que simula un error para demostrar tasa de errores
    /// </summary>
    [HttpGet("error-test")]
    public IActionResult ErrorTest([FromQuery] bool shouldFail = false)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";

        if (shouldFail)
        {
            _logger.LogError("Error simulado | CorrelationId: {CorrelationId}", correlationId);

            OpenTelemetryConfiguration.ErrorCounter.Add(1,
                new KeyValuePair<string, object?>("endpoint", "error-test"),
                new KeyValuePair<string, object?>("error_type", "SimulatedError"));

            return StatusCode(500, new
            {
                error = "Error simulado para testing",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }

        _logger.LogInformation("Error test ejecutado sin errores | CorrelationId: {CorrelationId}", correlationId);

        return Ok(new
        {
            message = "Sin errores",
            correlationId,
            timestamp = DateTime.UtcNow
        });
    }
}
