using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Api.Configuration;
using System.Diagnostics;
using Application.Common.Interfaces;

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
    private readonly ICacheMetricsService _cacheMetricsService;

    public ObservabilityController(
        ILogger<ObservabilityController> logger,
        ICacheMetricsService cacheMetricsService)
    {
        _logger = logger;
        _cacheMetricsService = cacheMetricsService;
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

    /// <summary>
    /// Endpoint para obtener métricas de cache (hit/miss rates)
    /// </summary>
    [HttpGet("cache-metrics")]
    public IActionResult GetCacheMetrics([FromQuery] string? pattern = null)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";

        try
        {
            var metrics = string.IsNullOrWhiteSpace(pattern)
                ? _cacheMetricsService.GetMetrics()
                : _cacheMetricsService.GetMetrics(pattern);

            _logger.LogInformation(
                "Cache metrics retrieved | CorrelationId: {CorrelationId} | Pattern: {Pattern} | HitRate: {HitRate:P2}",
                correlationId, pattern ?? "all", metrics.HitRate);

            return Ok(new
            {
                correlationId,
                timestamp = DateTime.UtcNow,
                pattern = pattern ?? "all",
                metrics = new
                {
                    totalHits = metrics.TotalHits,
                    totalMisses = metrics.TotalMisses,
                    totalRequests = metrics.TotalRequests,
                    hitRate = $"{metrics.HitRate:P2}",
                    missRate = $"{metrics.MissRate:P2}",
                    hitRateDecimal = metrics.HitRate,
                    missRateDecimal = metrics.MissRate,
                    hitsByPattern = metrics.HitsByPattern,
                    missesByPattern = metrics.MissesByPattern
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache metrics | CorrelationId: {CorrelationId}", correlationId);
            
            return StatusCode(500, new
            {
                error = "Error al obtener métricas de cache",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Endpoint para resetear las métricas de cache
    /// </summary>
    [HttpPost("cache-metrics/reset")]
    public IActionResult ResetCacheMetrics()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";

        try
        {
            _cacheMetricsService.Reset();

            _logger.LogInformation("Cache metrics reset | CorrelationId: {CorrelationId}", correlationId);

            return Ok(new
            {
                message = "Métricas de cache reseteadas correctamente",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting cache metrics | CorrelationId: {CorrelationId}", correlationId);
            
            return StatusCode(500, new
            {
                error = "Error al resetear métricas de cache",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
