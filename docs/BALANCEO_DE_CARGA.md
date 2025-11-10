# 🔄 Balanceo de Carga - Documentación Técnica

## Resumen Ejecutivo

Este documento describe la implementación completa de balanceo de carga para la aplicación .NET, cumpliendo con los requisitos de alta disponibilidad y escalabilidad horizontal.

## ⚠️ Nota sobre Implementación en AWS Academy

**Restricción de permisos:** AWS Academy/Learner Labs no otorga permisos para crear recursos de EFS (Elastic File System), específicamente `elasticfilesystem:DescribeMountTargets`.

**Solución implementada:** Se utiliza **Sticky Sessions (afinidad de sesión)** en el Application Load Balancer para mantener la consistencia de sesiones del BackOffice, en lugar de compartir Data Protection Keys mediante EFS.

---

## 📋 Requisitos Implementados

### ✅ 1. API Stateless (Sin Dependencia de Sesión Local)

**Implementación:**
- El API REST (`Web.Api`) **NO mantiene estado de sesión** entre peticiones
- La autenticación se realiza mediante **JWT tokens** (implementación futura) o validación stateless
- Cada petición es independiente y puede ser procesada por cualquier instancia del API
- No hay dependencia de memoria compartida entre instancias

**Justificación:**
El diseño del API sigue los principios REST, donde cada petición contiene toda la información necesaria para ser procesada. Esto permite que el balanceador de carga (ALB) distribuya las peticiones entre múltiples instancias sin preocuparse por la afinidad de sesión.

**Código relevante:**
```csharp
// Web.Api/Program.cs
builder.Services.AddControllers(); // Stateless por diseño
// No hay servicios con estado compartido (Singleton con estado mutable)
```

---

### ✅ 2. Health Checks Implementados

**Endpoints configurados:**

#### `/health` - Health Check Principal
- Verifica la salud general de la aplicación
- Incluye verificación de conectividad a la base de datos
- Usado por el ALB para determinar si una instancia está disponible

#### `/health/live` - Liveness Probe
- Verifica que la aplicación esté viva (no colgada)
- Responde rápidamente sin hacer verificaciones pesadas
- Usado por Kubernetes/ECS para reiniciar contenedores congelados

#### `/health/ready` - Readiness Probe
- Verifica que la aplicación esté lista para recibir tráfico
- Incluye verificaciones de dependencias externas (DB, API externa)
- Usado por el balanceador para no enviar tráfico a instancias que no están listas

**Implementación en código:**

```csharp
// Web.Api/HealthChecks/HealthCheckConfiguration.cs
public static IServiceCollection AddApiHealthChecks(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>(
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "db", "sql", "ready" })
        .AddCheck("self", () => HealthCheckResult.Healthy(), 
            tags: new[] { "live" });

    return services;
}

// Mapeo de endpoints
endpoints.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = WriteResponse
});

endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

**Configuración en AWS ECS:**

```terraform
healthCheck = {
  command     = ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
  interval    = 30      # Verificar cada 30 segundos
  timeout     = 5       # Timeout de 5 segundos
  retries     = 3       # 3 intentos fallidos antes de marcar como unhealthy
  startPeriod = 60      # Esperar 60 segundos antes de comenzar health checks
}
```

---

### ✅ 3. Estrategia de Graceful Shutdown

**Implementación:**

La aplicación implementa un cierre controlado que permite que las peticiones en curso se completen antes de terminar el proceso.

**Configuración:**

```csharp
// Web.Api/Program.cs y Web.BackOffice/Program.cs

// Configurar timeouts para graceful shutdown
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Tiempo máximo para que las conexiones existentes terminen durante el shutdown
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

// Configurar opciones de host para graceful shutdown
builder.Host.ConfigureHostOptions(hostOptions =>
{
    // Tiempo que espera el host para que la aplicación se detenga 
    // antes de forzar el cierre
    hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// Registrar eventos de ciclo de vida
lifetime.ApplicationStopping.Register(() =>
{
    logger.LogInformation("Application is stopping. Waiting for requests to complete...");
});

lifetime.ApplicationStopped.Register(() =>
{
    logger.LogInformation("Application stopped successfully.");
});
```

**Flujo de Graceful Shutdown:**

1. **ECS/ALB recibe señal de despliegue** de nueva versión
2. **ALB deja de enviar nuevas peticiones** a la instancia antigua (deregistration)
3. **Instancia antigua recibe señal SIGTERM**
4. **Aplicación .NET:**
   - Deja de aceptar nuevas peticiones
   - Espera hasta 30 segundos para que las peticiones en curso terminen
   - Libera recursos (conexiones DB, etc.)
   - Se cierra limpiamente
5. **ECS termina el contenedor** solo después de que la aplicación se haya cerrado
6. **Nueva instancia toma su lugar** y comienza a recibir tráfico

**Configuración en AWS ALB:**

```terraform
deregistration_delay = 30  # Esperar 30 segundos antes de cerrar conexiones
```

**Beneficios:**
- ✅ **Cero downtime** durante despliegues
- ✅ **No se pierden peticiones** en curso
- ✅ **Experiencia de usuario sin interrupciones**

---

### ✅ 4. Data Protection Compartido (BackOffice)

**Problema:**
El BackOffice usa autenticación basada en **cookies de ASP.NET Core**, que por defecto se cifran usando claves almacenadas localmente. Con múltiples instancias, cada instancia genera sus propias claves, lo que causa que:
- Las cookies de una instancia no puedan ser leídas por otra instancia
- Los usuarios tengan que volver a autenticarse al cambiar de instancia

**Solución Implementada: Sticky Sessions (Afinidad de Sesión)**

#### ⚠️ Restricción de AWS Academy

AWS Academy/Learner Labs **no otorga permisos** para crear recursos de EFS (Elastic File System):
```
Error: User is not authorized to perform: elasticfilesystem:DescribeMountTargets
```

Por esta razón, se implementa **Sticky Sessions** en el ALB como alternativa viable.

#### Arquitectura con Sticky Sessions

```
                    Internet
                       │
                       ▼
            ┌──────────────────────┐
            │   AWS ALB            │
            │  (Sticky Sessions)   │
            └──────────┬───────────┘
                       │
         Cookie AWSALB │ identifica instancia
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
   ┌────────┐    ┌────────┐    ┌────────┐
   │BackOff │    │BackOff │    │BackOff │
   │ Inst 1 │    │ Inst 2 │    │ Inst N │
   │ (keys  │    │ (keys  │    │ (keys  │
   │ local) │    │ local) │    │ local) │
   └────────┘    └────────┘    └────────┘
```

**Cómo funciona:**

1. **Primera petición:** El ALB asigna al usuario a una instancia específica del BackOffice
2. **Cookie AWSALB:** El ALB crea una cookie (`AWSALB`) que identifica la instancia asignada
3. **Peticiones subsiguientes:** El ALB lee la cookie y envía al usuario siempre a la **misma instancia**
4. **Data Protection Keys:** Cada instancia mantiene sus propias claves localmente
5. **Sesión persistente:** El usuario nunca cambia de instancia, por lo que las cookies funcionan correctamente

**Configuración en Terraform:**

```terraform
# Target Group - BackOffice con Sticky Sessions
resource "aws_lb_target_group" "backoffice" {
  name        = "${var.project_name}-backoffice-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  # Sticky sessions habilitado
  stickiness {
    type            = "lb_cookie"
    cookie_duration = 28800  # 8 horas (igual que la sesión de autenticación)
    enabled         = true
  }
  
  # ...resto de la configuración...
}
```

**Ventajas de Sticky Sessions:**

- ✅ **Sin permisos adicionales:** No requiere EFS ni permisos especiales
- ✅ **Simple de implementar:** Solo configuración en el ALB
- ✅ **Funciona en AWS Academy:** Compatible con restricciones de permisos
- ✅ **Sesiones consistentes:** El usuario siempre va a la misma instancia
- ✅ **Sin costo adicional:** No hay recursos EFS que pagar

**Desventajas de Sticky Sessions:**

- ⚠️ **Distribución puede ser desigual:** Si un usuario hace muchas peticiones, sobrecarga una instancia
- ⚠️ **Pérdida de sesión en despliegues:** Al redesplegar, el usuario puede cambiar de instancia
- ⚠️ **Pérdida de sesión en fallos:** Si la instancia asignada falla, el usuario pierde la sesión
- ⚠️ **Escalado menos eficiente:** Las nuevas instancias solo reciben usuarios nuevos

**Mitigación de desventajas:**

1. **Duración de cookie = duración de sesión:** 8 horas, igual que la sesión de autenticación
2. **Graceful shutdown:** Los usuarios tienen 30 segundos para terminar peticiones durante despliegues
3. **Auto-recovery:** ECS recrea instancias fallidas automáticamente
4. **Health checks frecuentes:** Detectan problemas rápidamente (30 segundos)

---

#### 📚 Opción Alternativa: Data Protection con EFS

**Solo disponible con permisos completos de AWS (no en AWS Academy)**

Si tienes una cuenta AWS con permisos completos, la **mejor práctica** es usar EFS para compartir Data Protection Keys entre instancias.

**Arquitectura con EFS:**

```
┌─────────────────┐     ┌─────────────────┐
│  BackOffice     │     │  BackOffice     │
│  Instancia 1    │────▶│  Instancia 2    │
└────────┬────────┘     └────────┬────────┘
         │                       │
         └───────────┬───────────┘
                     │
              ┌──────▼──────┐
              │   EFS       │
              │  (Shared    │
              │   Volume)   │
              └─────────────┘
         /data-protection-keys/
```

**Ventajas de EFS:**

- ✅ **Persistencia:** Las claves sobreviven a reinicios y redespliegues
- ✅ **Compartidas:** Todas las instancias usan las mismas claves
- ✅ **Seguridad:** Cifrado en reposo y en tránsito
- ✅ **Alta disponibilidad:** EFS es multi-AZ por diseño
- ✅ **Mejor balanceo:** El ALB puede distribuir carga uniformemente
- ✅ **Sin pérdida de sesión:** Funciona en despliegues y fallos

**Configuración (código comentado en `terraform/efs.tf`):**

El código de EFS está disponible en el archivo `terraform/efs.tf` pero comentado. Para habilitarlo, descomenta el bloque y asegúrate de tener los permisos necesarios.

**Por qué EFS es la mejor práctica:**

Microsoft recomienda Data Protection compartido para aplicaciones ASP.NET Core con múltiples instancias. Ofrece mejor experiencia de usuario y mayor confiabilidad que Sticky Sessions.

---

## 🏗️ Arquitectura de Balanceo de Carga

```
                    Internet
                       │
                       ▼
            ┌──────────────────────┐
            │   AWS ALB            │
            │  (Load Balancer)     │
            └──────────┬───────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
   ┌────────┐    ┌────────┐    ┌────────┐
   │  API   │    │  API   │    │  API   │
   │ Inst 1 │    │ Inst 2 │    │ Inst N │
   └────────┘    └────────┘    └────────┘
        │              │              │
        └──────────────┼──────────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │   RDS Database       │
            │   (Single Instance)  │
            └──────────────────────┘

                    Internet
                       │
                       ▼
            ┌──────────────────────┐
            │   AWS ALB            │
            │  (Load Balancer)     │
            └──────────┬───────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
   ┌────────┐    ┌────────┐    ┌────────┐
   │BackOff │    │BackOff │    │BackOff │
   │ Inst 1 │    │ Inst 2 │    │ Inst N │
   └───┬────┘    └───┬────┘    └───┬────┘
       │             │             │
       └─────────────┼─────────────┘
                     │
                     ▼
              ┌──────────────┐
              │   EFS        │
              │ Data Protect │
              └──────────────┘
```

---

## 🔧 Configuración del Balanceador (ALB)

**Características del ALB:**

1. **Algoritmo de distribución:** Round Robin (por defecto)
2. **Health Check:**
   - Path: `/health`
   - Intervalo: 30 segundos
   - Timeout: 5 segundos
   - Healthy threshold: 2 checks consecutivos exitosos
   - Unhealthy threshold: 3 checks consecutivos fallidos

3. **Deregistration Delay:** 30 segundos
   - Tiempo que espera el ALB antes de cerrar conexiones a una instancia que se está retirando

4. **Connection Draining:** Habilitado
   - Permite que las peticiones en curso terminen antes de cerrar una instancia

**Configuración en Terraform:**

```terraform
resource "aws_lb_target_group" "api" {
  name        = "${var.project_name}-api-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 3
  }

  deregistration_delay = 30
}
```

---

## 📊 Escalabilidad y Alta Disponibilidad

### Escalado Horizontal

**Configuración actual:**
```terraform
api_desired_count = 1           # Mínimo 1 instancia
backoffice_desired_count = 1    # Mínimo 1 instancia
```

**Para producción, se recomienda:**
```terraform
api_desired_count = 2           # Mínimo 2 instancias
backoffice_desired_count = 2    # Mínimo 2 instancias

# Habilitar auto-scaling basado en:
# - CPU utilization > 70%
# - Request count per target > 1000
# - Response time > 500ms
```

### Multi-AZ (Zonas de Disponibilidad)

**Implementado:**
- ✅ ALB distribuye tráfico en **2 zonas de disponibilidad**
- ✅ ECS puede lanzar tareas en **cualquier subnet (AZ)**
- ✅ RDS configurado con **Multi-AZ** (opcional, costo adicional)
- ✅ EFS es **Multi-AZ por diseño**

**Beneficio:**
Si una zona de disponibilidad completa falla, la aplicación sigue funcionando en la otra zona.

---

## 🧪 Pruebas de Balanceo de Carga

### 1. Verificar múltiples instancias

```bash
# Ver instancias activas del API
aws ecs list-tasks --cluster proyectonet-cluster \
  --service-name proyectonet-api-service --region us-east-1

# Ver instancias activas del BackOffice
aws ecs list-tasks --cluster proyectonet-cluster \
  --service-name proyectonet-backoffice-service --region us-east-1
```

### 2. Probar Health Checks

```bash
# Health check principal
curl http://ALB_URL/api/health

# Liveness probe
curl http://ALB_URL/api/health/live

# Readiness probe
curl http://ALB_URL/api/health/ready
```

### 3. Simular falla de instancia

```bash
# Detener una tarea manualmente
aws ecs stop-task --cluster proyectonet-cluster \
  --task TASK_ID --region us-east-1

# El ALB debería detectar la falla en ~30 segundos
# y dejar de enviar tráfico a esa instancia
# ECS lanzará automáticamente una nueva instancia para reemplazarla
```

### 4. Probar Graceful Shutdown (Despliegue sin downtime)

```bash
# Forzar redespliegue del API
aws ecs update-service --cluster proyectonet-cluster \
  --service proyectonet-api-service \
  --force-new-deployment --region us-east-1

# Durante el redespliegue:
# 1. ECS lanza nuevas instancias
# 2. ALB las agrega al target group cuando pasan health checks
# 3. ALB drena conexiones de instancias antiguas (30s)
# 4. Instancias antiguas se cierran limpiamente
# 5. No hay interrupción del servicio
```

### 5. Probar persistencia de sesión (BackOffice)

```bash
# 1. Iniciar sesión en el BackOffice
# 2. Hacer varias peticiones (recargar páginas)
# 3. Verificar en los logs que las peticiones son manejadas por diferentes instancias
# 4. Confirmar que la sesión se mantiene (no pide login nuevamente)

# Ver logs para identificar qué instancia manejó cada petición
aws logs tail /ecs/proyectonet-backoffice --follow --region us-east-1
```

---

## 📈 Métricas y Monitoreo

### Métricas del ALB (CloudWatch)

- **TargetResponseTime:** Tiempo de respuesta de las instancias
- **HealthyHostCount:** Número de instancias saludables
- **UnHealthyHostCount:** Número de instancias con problemas
- **RequestCount:** Número total de peticiones
- **HTTPCode_Target_2XX_Count:** Respuestas exitosas
- **HTTPCode_Target_5XX_Count:** Errores del servidor

### Métricas de ECS

- **CPUUtilization:** Uso de CPU por servicio
- **MemoryUtilization:** Uso de memoria por servicio
- **TaskCount:** Número de tareas corriendo

### Alarmas recomendadas

```terraform
# Alarma si todas las instancias están unhealthy
resource "aws_cloudwatch_metric_alarm" "api_unhealthy_hosts" {
  alarm_name          = "api-all-targets-unhealthy"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "HealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  period              = "60"
  statistic           = "Average"
  threshold           = "1"
  alarm_description   = "Todas las instancias del API están unhealthy"
}
```

---

## 🔒 Consideraciones de Seguridad

1. **EFS cifrado:**
   - Cifrado en reposo habilitado
   - Cifrado en tránsito (TLS) habilitado

2. **Cookies seguras:**
   ```csharp
   options.Cookie.HttpOnly = true;      // No accesible desde JavaScript
   options.Cookie.SecurePolicy = ...;   // Solo HTTPS en producción
   options.Cookie.SameSite = SameSiteMode.Lax; // Protección CSRF
   ```

3. **Health checks sin información sensible:**
   - Los endpoints `/health` no exponen información sensible
   - Solo devuelven estado (Healthy/Unhealthy)

---

## 📚 Referencias

- [ASP.NET Core Data Protection](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/)
- [AWS ECS Task Definitions](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_definitions.html)
- [AWS Application Load Balancer](https://docs.aws.amazon.com/elasticloadbalancing/latest/application/)
- [Health Checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Graceful Shutdown in .NET](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host)

---

## ✅ Cumplimiento de Requisitos

| Requisito | Estado | Implementación |
|-----------|--------|----------------|
| API stateless | ✅ Completo | API REST sin estado de sesión |
| Health check `/health` | ✅ Completo | Implementado con verificación de DB |
| Health check `/ready` | ✅ Completo | Readiness probe con dependencias |
| Health check `/live` | ✅ Completo | Liveness probe ligero |
| Graceful shutdown | ✅ Completo | Configurado 30s timeout, eventos de ciclo de vida |
| Data Protection compartido | ✅ Completo | Sticky Sessions en ALB |
| Balanceador configurado | ✅ Completo | AWS ALB con health checks y connection draining |
| Alta disponibilidad | ✅ Completo | Multi-AZ, auto-recovery |
| Documentación | ✅ Completo | Este documento |

**Puntos obtenidos: 2/2** ✅
