# Implementación Docker - Proyecto.NET

Documentación técnica sobre la implementación de Docker en el proyecto, cubriendo arquitectura, configuración y uso básico.

---

## Qué se Levanta

El proyecto utiliza Docker Compose para orquestar 4 servicios principales:

### 1. SQL Server (proyectonet-sqlserver)
Base de datos SQL Server 2022 que almacena toda la información del sistema.
- **Puerto**: 1433
- **Imagen**: mcr.microsoft.com/mssql/server:2022-latest
- **Volumen**: `sqlserver-data` (persistencia de datos)
- **Función**: Almacenar usuarios, roles, noticias, eventos de acceso, etc.

### 2. Web.Api (proyectonet-web-api)
API REST principal que expone todos los endpoints del sistema.
- **Puerto**: 5000 (configurable con `API_HTTP_PORT`)
- **Swagger**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- **Función**: Backend principal, manejo de autenticación, CRUD de entidades

### 3. Web.BackOffice (proyectonet-backoffice)
Aplicación web para administración interna del sistema.
- **Puerto**: 5001 (configurable con `BACKOFFICE_HTTP_PORT`)
- **Health Check**: http://localhost:5001/health
- **Función**: Panel de administración para gestionar usuarios, roles, configuración

### 4. Web.FrontOffice (proyectonet-frontoffice)
Aplicación web orientada a usuarios finales.
- **Puerto**: 5002 (configurable con `FRONTOFFICE_HTTP_PORT`)
- **Health Check**: http://localhost:5002/health
- **Función**: Interfaz pública para consumir servicios del sistema

Todos los servicios se comunican a través de una red interna Docker (`proyectonet-network`).

---

## Propósito de Archivos Docker

### Dockerfile
Build multi-stage que compila y empaqueta las 3 aplicaciones .NET (Api, BackOffice, FrontOffice) en imágenes optimizadas. Usa 6 stages para minimizar el tamaño final de las imágenes.

### docker-compose.yml
Configuración base que define los 4 servicios, sus dependencias, networking y configuración común a todos los ambientes.

### docker-compose.override.yml
Overrides para ambiente de desarrollo. Se aplica automáticamente con `docker-compose up`. Incluye:
- Swagger habilitado
- Hot reload
- CORS permisivo
- Seeding de base de datos

### docker-compose.prod.yml
Configuración específica para producción. Se usa explícitamente con `-f docker-compose.yml -f docker-compose.prod.yml`. Incluye:
- Swagger deshabilitado
- Múltiples réplicas de API
- CORS restrictivo
- Logging estructurado

### .env
Variables de entorno locales (no versionado). Contiene configuración específica de tu máquina como puertos y contraseñas.

### .env.example
Plantilla de variables de entorno. Copiar a `.env` y configurar según necesidad.

### run-local.ps1
Script principal de automatización para desarrollo local. Maneja todo el ciclo de vida: validación, build, inicio, health checks y carga de datos.

---

## Inicio Rápido

### 1. Configurar Variables
```powershell
cp .env.example .env
# Editar .env y configurar SQL_SERVER_PASSWORD
```

### 2. Iniciar Aplicación
```powershell
.\run-local.ps1
```

El script automáticamente:
- Valida la configuración
- Construye las imágenes Docker
- Inicia todos los servicios
- Espera a que estén healthy
- Muestra las URLs de acceso

### 3. Acceder
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger (solo desarrollo)
- BackOffice: http://localhost:5001
- FrontOffice: http://localhost:5002

### 4. Datos de Prueba

En modo desarrollo, el sistema carga automáticamente:
- **Tenant Demo**: Tenant de prueba para desarrollo
- **Usuario Admin**: `admin1@backoffice.com` / `Admin123!`

Los datos se cargan automáticamente al iniciar la API si `SEED_DATABASE=true` (configuración por defecto en desarrollo).

---

## Variables de Entorno Clave

| Variable | Default | Descripción |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Production | Ambiente (Development/Production) |
| `SQL_SERVER_PASSWORD` | (requerido) | Contraseña SA de SQL Server |
| `DB_NAME` | ProyectoNetDb | Nombre de la base de datos |
| `API_HTTP_PORT` | 5000 | Puerto HTTP de la API |
| `BACKOFFICE_HTTP_PORT` | 5001 | Puerto del BackOffice |
| `FRONTOFFICE_HTTP_PORT` | 5002 | Puerto del FrontOffice |
| `SEED_DATABASE` | false | Habilitar datos de prueba |

Ver `.env.example` para la lista completa de 15 variables configurables.

---

## Comandos Comunes

### Gestión Básica
```powershell
.\run-local.ps1                   # Iniciar desarrollo
.\run-local.ps1 -Clean            # Limpiar e iniciar
.\run-local.ps1 -Foreground       # Ver logs en tiempo real
docker-compose ps                 # Ver estado
docker-compose logs -f            # Ver logs
docker-compose down               # Detener servicios
docker-compose restart web-api    # Reiniciar servicio
```

### Build y Actualización
```powershell
.\run-local.ps1 -Clean            # Reconstruir todo
docker-compose build              # Solo construir imágenes
docker-compose up -d --build      # Reconstruir e iniciar
```

### Limpieza
```powershell
docker-compose down -v            # Detener y eliminar volúmenes
docker system prune -a --volumes  # Limpieza completa de Docker
```

---

## Arquitectura Técnica

### Networking
Todos los servicios se comunican a través de `proyectonet-network` (bridge network). Resolución DNS automática por nombre de servicio:
- `sqlserver` → SQL Server
- `web-api` → API REST
- `web-backoffice` → BackOffice
- `web-frontoffice` → FrontOffice

### Persistencia
Un volumen nombrado `sqlserver-data` almacena la base de datos persistentemente. Los datos sobreviven a recreaciones de contenedores.

### Health Checks
Cada servicio implementa health checks automáticos:
- SQL Server: Verifica conexión con `sqlcmd`
- Aplicaciones .NET: Endpoint `/health` con Entity Framework check

### Multi-stage Build
El Dockerfile usa 6 stages:
1. **base**: Runtime base de .NET
2. **build**: SDK para compilación
3. **publish-api**: Build de Web.Api
4. **publish-backoffice**: Build de Web.BackOffice
5. **publish-frontoffice**: Build de Web.FrontOffice
6. **final**: Imagen final runtime-only

Esto reduce el tamaño de las imágenes finales eliminando el SDK y archivos de compilación.

---

## Desarrollo vs Producción

### Desarrollo
```powershell
.\run-local.ps1
```
- Environment: Development
- Swagger: Habilitado
- Hot Reload: Activado
- CORS: Permisivo (localhost)
- Seeding: Habilitado
- Límites: Relajados

### Producción
```powershell
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```
- Environment: Production
- Swagger: Deshabilitado
- Hot Reload: Desactivado
- CORS: Restrictivo (whitelist)
- Seeding: Deshabilitado
- Límites: Estrictos
- Réplicas: 2 instancias de API

---

## Troubleshooting Básico

### SQL Server no inicia
```powershell
# Verificar contraseña cumple requisitos (mín 8 chars, mayúsculas, números, símbolos)
docker logs proyectonet-sqlserver
```

### Puerto en uso
```powershell
# Cambiar puerto en .env
API_HTTP_PORT=5100
docker-compose up -d
```

### Base de datos corrupta
```powershell
# Recrear desde cero
docker-compose down -v
docker-compose up -d
```

### Health check failed
```powershell
# Verificar logs del servicio
docker-compose logs web-api

# Verificar health endpoint manualmente
curl http://localhost:5000/health
```

---

## Scripts de Automatización

### run-local.ps1
Script principal recomendado para desarrollo local. Ejecuta todo el flujo completo:
```powershell
.\run-local.ps1                    # Inicio normal con carga de datos
.\run-local.ps1 -Clean             # Limpieza completa antes de iniciar
.\run-local.ps1 -NoBuild           # Saltar construcción de imágenes
.\run-local.ps1 -Foreground        # Ejecutar en primer plano
```

Automáticamente:
1. Verifica que Docker esté ejecutándose
2. Inicializa archivo .env si no existe
3. Valida configuración de variables
4. Construye imágenes (opcional)
5. Inicia servicios en modo detached
6. Espera a que todos los servicios estén healthy
7. Muestra URLs de acceso

Los datos de prueba (Tenant Demo y usuario admin) se cargan automáticamente desde el código C# al iniciar la API en modo desarrollo.

### validate-env.ps1
Valida que el archivo `.env` existe y contiene todas las variables requeridas con valores válidos.

---

## Seguridad

- Contraseñas en `.env` (no versionado)
- Usuarios no-root en contenedores
- CORS configurable por ambiente
- Health checks sin exponer información sensible
- TLS configurable para producción
- Validación de variables requeridas

---

**Última actualización**: 3 de noviembre de 2025
