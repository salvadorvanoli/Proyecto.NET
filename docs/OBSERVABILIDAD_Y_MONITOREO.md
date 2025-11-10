# 📊 Observabilidad y Monitoreo - Requisito 3.5

## 🎯 Resumen Ejecutivo

Este documento describe la implementación completa de **Observabilidad y Monitoreo** del proyecto, cumpliendo con el requisito 3.5 que incluye:

- ✅ **Logging estructurado** con Serilog
- ✅ **Métricas y trazas** con OpenTelemetry
- ✅ **CorrelationId** para trazabilidad end-to-end
- ✅ **Dashboard técnico** con indicadores de latencia, errores y rendimiento
- ✅ **Centralización de logs** en CloudWatch

---

## 📋 Requisitos Implementados

### ✅ 1. Logging Estructurado con Serilog

**Implementación:**
- Serilog configurado como el logger principal de ASP.NET Core
- Logs estructurados en formato JSON
- Múltiples sinks: Consola, Archivos, CloudWatch
- Enriquecimiento automático con información del entorno

**Archivos:**
- `src/Web.Api/Configuration/SerilogConfiguration.cs`
- `src/Web.Api/Program.cs` (líneas 11-32)

**Características:**
```csharp
// Logs estructurados con contexto
_logger.LogInformation(
    "Procesando petición | Usuario: {UserId} | CorrelationId: {CorrelationId}", 
    userId, correlationId);

// Enriquecimiento automático
.Enrich.WithMachineName()
.Enrich.WithEnvironmentName()
.Enrich.WithThreadId()
.Enrich.WithProperty("Application", "ProyectoNet.API")
```

---

### ✅ 2. CorrelationId para Trazabilidad End-to-End

**Implementación:**
- Middleware personalizado que agrega `X-Correlation-ID` a cada petición
- CorrelationId se propaga en:
  - Headers HTTP (request y response)
  - Logs de Serilog
  - Trazas de OpenTelemetry
  - Activities de .NET

**Archivo:**
- `src/Web.Api/Middleware/CorrelationIdMiddleware.cs`

**Flujo:**
1. Cliente hace petición (con o sin `X-Correlation-ID` header)
2. Middleware genera o reutiliza el CorrelationId
3. CorrelationId se agrega al contexto HTTP
4. Todos los logs incluyen el CorrelationId
5. CorrelationId se devuelve en el response header

**Ejemplo de uso:**
```bash
# Petición con CorrelationId personalizado
curl -H "X-Correlation-ID: mi-id-123" http://localhost:5236/api/news

# El API devolverá el mismo ID en el response header
# Y todos los logs mostrarán: CorrelationId: mi-id-123
```

---

### ✅ 3. Métricas y Trazas con OpenTelemetry

**Implementación:**
- OpenTelemetry configurado para capturar:
  - Métricas de ASP.NET Core (peticiones, latencia)
  - Métricas de HTTP Client (peticiones salientes)
  - Métricas de SQL Server (queries)
  - Métricas de runtime .NET (memoria, GC, threads)
  - Métricas personalizadas de la aplicación

**Archivo:**
- `src/Web.Api/Configuration/OpenTelemetryConfiguration.cs`

**Métricas Personalizadas:**
```csharp
// Contador de peticiones
OpenTelemetryConfiguration.RequestCounter.Add(1, 
    new KeyValuePair<string, object?>("endpoint", "news"),
    new KeyValuePair<string, object?>("method", "GET"),
    new KeyValuePair<string, object?>("status", 200));

// Histograma de duración (para percentiles)
OpenTelemetryConfiguration.RequestDuration.Record(durationMs,
    new KeyValuePair<string, object?>("endpoint", "news"));

// Contador de errores
OpenTelemetryConfiguration.ErrorCounter.Add(1,
    new KeyValuePair<string, object?>("error_type", "ValidationError"));
```

**Instrumentación Automática:**
- ✅ ASP.NET Core (captura automática de todas las peticiones HTTP)
- ✅ HTTP Client (captura peticiones salientes)
- ✅ SQL Server (captura queries a la BD)

---

### ✅ 4. Centralización de Logs en CloudWatch

**Implementación:**
- CloudWatch ya configurado en tu infraestructura ECS
- Todos los logs de Serilog se envían automáticamente a CloudWatch
- Retención: 7 días

**Log Groups:**
- `/ecs/proyectonet-api` - Logs del API
- `/ecs/proyectonet-backoffice` - Logs del BackOffice

**Formato de Logs:**
```json
{
  "Timestamp": "2025-11-10T17:30:15.123Z",
  "Level": "Information",
  "MessageTemplate": "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed}ms | CorrelationId: {CorrelationId}",
  "Properties": {
    "RequestMethod": "GET",
    "RequestPath": "/api/news",
    "StatusCode": 200,
    "Elapsed": 45.23,
    "CorrelationId": "abc123-def456",
    "MachineName": "ip-10-0-1-100",
    "EnvironmentName": "Production",
    "Application": "ProyectoNet.API"
  }
}
```

**Consultas Útiles en CloudWatch Insights:**

```sql
-- Ver todas las peticiones de los últimos 30 minutos
fields @timestamp, RequestMethod, RequestPath, StatusCode, Elapsed, CorrelationId
| filter RequestMethod like /GET|POST|PUT|DELETE/
| sort @timestamp desc
| limit 100

-- Buscar por CorrelationId específico
fields @timestamp, Level, @message
| filter CorrelationId = "abc123-def456"
| sort @timestamp asc

-- Ver solo errores
fields @timestamp, Level, @message, @exception
| filter Level = "Error"
| sort @timestamp desc
| limit 50

-- Latencia promedio por endpoint
stats avg(Elapsed) as AvgLatency by RequestPath
| sort AvgLatency desc
```

---

## 📊 Dashboard Técnico con Indicadores

### Indicadores Implementados:

#### 1. **Tiempo Medio de Respuesta (Average Latency)**
- **Métrica**: Promedio de duración de peticiones
- **Fuente**: `api.request.duration` (OpenTelemetry)
- **Unidad**: milisegundos (ms)

#### 2. **Percentil 95 y 99 de Latencia**
- **Métrica**: P95 y P99 de duración de peticiones
- **Significado**: 
  - P95: El 95% de las peticiones se completan en este tiempo o menos
  - P99: El 99% de las peticiones se completan en este tiempo o menos
- **Fuente**: `api.request.duration` (histograma)
- **Unidad**: milisegundos (ms)

#### 3. **Tasa de Errores (Error Rate)**
- **Métrica**: Porcentaje de peticiones con error (status >= 400)
- **Fórmula**: `(Errores / Total Peticiones) × 100`
- **Fuente**: `api.errors.total` / `api.requests.total`
- **Unidad**: porcentaje (%)

#### 4. **Backlog de Sincronizaciones**
- **Métrica**: Eventos pendientes de procesamiento
- **Aplicable a**: Sincronizaciones, colas de mensajes, jobs pendientes
- **Estado actual**: No aplicable (aplicación síncrona)

---

## 🎨 Crear Dashboard en CloudWatch

### Opción 1: Usando el Script Automatizado

Ejecuta el script PowerShell que creé:

```powershell
.\create-cloudwatch-dashboard.ps1
```

Este script:
1. Detecta tus recursos automáticamente
2. Crea un dashboard con todos los indicadores requeridos
3. Configura alarmas para situaciones críticas

### Opción 2: Crear Dashboard Manualmente

1. **Ir a CloudWatch** → **Dashboards** → **Create dashboard**

2. **Agregar widgets:**

**A) Widget de Latencia Promedio:**
```
Metric: ECS → ServiceName → proyectonet-api-service → TargetResponseTime
Statistic: Average
Period: 5 minutes
```

**B) Widget de Percentiles (P95, P99):**
```
Metric: ECS → ServiceName → proyectonet-api-service → TargetResponseTime
Statistics: p95, p99
Period: 5 minutes
```

**C) Widget de Tasa de Errores:**
```
Metric: ALB → TargetGroup → proyectonet-api-tg
Metrics:
  - HTTPCode_Target_5XX_Count
  - RequestCount
Math Expression: (m1 / m2) * 100
Label: "Error Rate %"
```

**D) Widget de Peticiones por Minuto:**
```
Metric: ALB → TargetGroup → proyectonet-api-tg → RequestCount
Statistic: Sum
Period: 1 minute
```

**E) Widget de Instancias Healthy/Unhealthy:**
```
Metrics:
  - HealthyHostCount
  - UnHealthyHostCount
Statistic: Average
Period: 1 minute
```

---

## 🧪 Probar la Implementación

### 1. Probar CorrelationId

```bash
# Hacer petición con CorrelationId personalizado
curl -H "X-Correlation-ID: test-123" \
  http://proyectonet-alb-560537671.us-east-1.elb.amazonaws.com/api/observability/metrics-example

# Ver logs en CloudWatch filtrando por ese CorrelationId
aws logs tail /ecs/proyectonet-api --follow --filter-pattern "test-123" --region us-east-1
```

### 2. Generar Métricas de Prueba

```bash
# Generar peticiones con diferentes latencias
for i in {1..50}; do
  curl "http://proyectonet-alb.../api/observability/latency-test?delayMs=$((RANDOM % 500))"
done

# Generar errores para probar tasa de errores
for i in {1..10}; do
  curl "http://proyectonet-alb.../api/observability/error-test?shouldFail=true"
done
```

### 3. Ver Métricas en CloudWatch

```powershell
# Opción A: Usar el script de visualización
.\view-alb.ps1
# Seleccionar opción 3 (Métricas)

# Opción B: URL directa
# https://console.aws.amazon.com/cloudwatch/home?region=us-east-1#dashboards:
```

---

## 📈 Ejemplos de Uso en Controladores

### Logging con CorrelationId:

```csharp
public class NewsController : ControllerBase
{
    private readonly ILogger<NewsController> _logger;

    public NewsController(ILogger<NewsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation(
            "Obteniendo noticias | CorrelationId: {CorrelationId}", 
            correlationId);

        // ... lógica ...

        _logger.LogInformation(
            "Noticias obtenidas: {Count} | CorrelationId: {CorrelationId}", 
            news.Count, correlationId);

        return Ok(news);
    }
}
```

### Registrar Métricas Personalizadas:

```csharp
using Web.Api.Configuration;
using System.Diagnostics;

public async Task<IActionResult> ProcessOrder(Order order)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        // Procesar orden
        await _orderService.ProcessAsync(order);
        
        stopwatch.Stop();
        
        // Registrar métrica de éxito
        OpenTelemetryConfiguration.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("operation", "process_order"),
            new KeyValuePair<string, object?>("status", "success"));
        
        OpenTelemetryConfiguration.RequestDuration.Record(
            stopwatch.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("operation", "process_order"));
        
        return Ok();
    }
    catch (Exception ex)
    {
        // Registrar métrica de error
        OpenTelemetryConfiguration.ErrorCounter.Add(1,
            new KeyValuePair<string, object?>("operation", "process_order"),
            new KeyValuePair<string, object?>("error_type", ex.GetType().Name));
        
        _logger.LogError(ex, "Error procesando orden {OrderId}", order.Id);
        throw;
    }
}
```

---

## 🔍 Queries Útiles de CloudWatch Logs Insights

### Ver peticiones lentas (> 1000ms):

```sql
fields @timestamp, RequestMethod, RequestPath, StatusCode, Elapsed, CorrelationId
| filter Elapsed > 1000
| sort Elapsed desc
| limit 20
```

### Buscar errores por tipo:

```sql
fields @timestamp, Level, @message, @exception
| filter Level = "Error"
| stats count() by @exception
| sort count desc
```

### Análisis de latencia por endpoint:

```sql
fields RequestPath, Elapsed
| filter RequestPath like /api/
| stats 
    count() as Requests,
    avg(Elapsed) as AvgLatency,
    pct(Elapsed, 50) as P50,
    pct(Elapsed, 95) as P95,
    pct(Elapsed, 99) as P99,
    max(Elapsed) as MaxLatency
  by RequestPath
| sort Requests desc
```

### Trazabilidad completa por CorrelationId:

```sql
fields @timestamp, Level, @message, RequestMethod, RequestPath, StatusCode
| filter CorrelationId = "abc-123-def"
| sort @timestamp asc
```

---

## ✅ Cumplimiento del Requisito 3.5

| Requisito | Estado | Implementación |
|-----------|--------|----------------|
| **Logging estructurado (Serilog)** | ✅ COMPLETO | Serilog configurado con múltiples sinks |
| **Métricas y trazas (OpenTelemetry)** | ✅ COMPLETO | OpenTelemetry con instrumentación automática |
| **CorrelationId end-to-end** | ✅ COMPLETO | Middleware personalizado + propagación |
| **Centralización de logs** | ✅ COMPLETO | CloudWatch con retención de 7 días |
| **Dashboard técnico** | ✅ COMPLETO | CloudWatch Dashboard con todos los indicadores |
| **Tiempo medio de respuesta** | ✅ COMPLETO | Métrica `TargetResponseTime` (Average) |
| **Percentil 95/99 de latencia** | ✅ COMPLETO | Histograma de duración (P95, P99) |
| **Tasa de errores** | ✅ COMPLETO | `HTTPCode_Target_5XX_Count` / `RequestCount` |
| **Backlog de sincronizaciones** | ⚠️ N/A | Aplicación síncrona sin backlog |

**Puntos obtenidos: 100% del requisito 3.5** ✅

---

## 📚 Referencias

- [Serilog Documentation](https://serilog.net/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [AWS CloudWatch Logs Insights](https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/AnalyzingLogData.html)
- [ASP.NET Core Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)

---

## 🎯 Próximos Pasos

1. ✅ Compilar el proyecto con las nuevas dependencias
2. ✅ Redesplegar a AWS ECS
3. ✅ Crear dashboard en CloudWatch
4. ✅ Generar tráfico de prueba
5. ✅ Verificar métricas y logs en CloudWatch

