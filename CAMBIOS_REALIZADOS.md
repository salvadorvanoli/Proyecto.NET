# Cambios Realizados en el CI/CD y Configuración de AWS

## Resumen de Cambios

Se han realizado los siguientes cambios para asegurar que las aplicaciones (BackOffice, FrontOffice y API) se conecten correctamente cuando se despliegan en AWS con el Application Load Balancer (ALB):

## 1. Configuración del Application Load Balancer (ALB)

**Archivo modificado:** `terraform/alb.tf`

### Cambios:
- ✅ **Agregada regla para la API** con prioridad 50 que redirecciona las peticiones `/api` y `/api/*` al target group de la API
- ✅ **Mejorada la regla del FrontOffice** para incluir tanto `/frontoffice` como `/frontoffice/*`
- ✅ La regla del BackOffice con prioridad 100 captura todas las demás peticiones (`/*`)

### Orden de prioridad de las reglas:
1. **Prioridad 50**: `/api` y `/api/*` → API
2. **Prioridad 75**: `/frontoffice` y `/frontoffice/*` → FrontOffice
3. **Prioridad 100**: `/*` → BackOffice (default)

## 2. Configuración de ECS Task Definitions

**Archivo modificado:** `terraform/ecs.tf`

### Cambios en la API:
- ✅ Agregada variable de entorno `PATH_BASE=/api` para que la API funcione correctamente bajo el path `/api`
- ✅ La variable `SEED_DATABASE=true` ya estaba configurada correctamente

### Cambios en el FrontOffice:
- ✅ Agregada variable de entorno `PATH_BASE=/frontoffice`
- ✅ Cambiada la variable `API_BASE_URL` de `http://{ALB_DNS}` a `http://{ALB_DNS}/api` para que apunte al endpoint correcto de la API

### Cambios en el BackOffice:
- ✅ Mantiene `API_BASE_URL=http://{ALB_DNS}` (sin /api) porque el BackOffice hace las llamadas internamente agregando el path

## 3. Configuración de las Aplicaciones ASP.NET Core

### Web.Api/Program.cs
**Cambios:**
- ✅ Agregado soporte para `UsePathBase()` que lee la variable de entorno `PATH_BASE`
- ✅ Esto permite que la API funcione correctamente cuando está detrás de un ALB con path `/api`

### Web.FrontOffice/Program.cs
**Cambios:**
- ✅ Actualizada la configuración de `API_BASE_URL` para usar la misma lógica que el BackOffice
- ✅ Ahora lee primero `API_BASE_URL` de la configuración o variable de entorno, con fallback a `ApiSettings:BaseUrl`
- ✅ Agregado soporte para `UsePathBase()` con la variable de entorno `PATH_BASE`
- ✅ Agregado logging para ver qué URL del API se está usando

## 4. CI/CD Pipeline (GitHub Actions)

**Archivo modificado:** `.github/workflows/ci-cd-pipeline.yml`

### Cambios:
- ✅ El job `report` ya estaba configurado correctamente para mostrar las 3 URLs al final del workflow
- ✅ Se mantienen los outputs para BackOffice, FrontOffice y API
- ✅ Las URLs se guardan en un artifact llamado `deploy-urls.txt`

## 5. DatabaseSeeder

**Verificación:**
- ✅ El `DatabaseSeeder` ya está correctamente configurado en `src/Infrastructure/Data/DatabaseSeeder.cs`
- ✅ El seeding se ejecuta automáticamente cuando `SEED_DATABASE=true` (ya configurado en ECS para la API)
- ✅ El seed se ejecuta en el `Program.cs` de la API después de las migraciones

## URLs de Despliegue

Después del despliegue, las aplicaciones estarán disponibles en:

- **BackOffice**: `http://{ALB_DNS}/` (ruta raíz)
- **FrontOffice**: `http://{ALB_DNS}/frontoffice`
- **API**: `http://{ALB_DNS}/api`

## Cómo Funciona la Conexión

1. **BackOffice → API**: 
   - El BackOffice usa `API_BASE_URL=http://{ALB_DNS}` 
   - Las llamadas se hacen internamente agregando `/api` en el código

2. **FrontOffice → API**: 
   - El FrontOffice usa `API_BASE_URL=http://{ALB_DNS}/api`
   - El `PATH_BASE=/frontoffice` permite que el FrontOffice funcione bajo `/frontoffice`

3. **Usuarios → Aplicaciones**:
   - El ALB redirige las peticiones según el path:
     - `/api/*` → API (contenedor en puerto 8080)
     - `/frontoffice/*` → FrontOffice (contenedor en puerto 8080)
     - `/*` → BackOffice (contenedor en puerto 8080)

## Testing

Para verificar que todo funciona correctamente después del despliegue:

1. **Verificar el BackOffice**: Accede a `http://{ALB_DNS}/`
2. **Verificar el FrontOffice**: Accede a `http://{ALB_DNS}/frontoffice`
3. **Verificar la API**: Accede a `http://{ALB_DNS}/api/health`
4. **Verificar logs**: Usa el script `view-logs.ps1` para ver los logs de cada servicio

## Próximos Pasos

1. Ejecutar el workflow de GitHub Actions para desplegar los cambios
2. Verificar que las 3 aplicaciones se levanten correctamente
3. Verificar que el FrontOffice pueda comunicarse con la API
4. Verificar que el DatabaseSeeder haya poblado la base de datos correctamente

