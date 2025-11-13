# Scripts de Automatización

Scripts PowerShell para facilitar el desarrollo y despliegue del proyecto con Docker.

## Scripts Disponibles

### validate-env.ps1
Valida que el archivo `.env` contiene todas las variables requeridas con valores válidos.

**Uso:**
```powershell
.\scripts\validate-env.ps1              # Validación desarrollo
.\scripts\validate-env.ps1 -Production  # Validación producción
```

**Validaciones:**
- Archivo .env existe
- Variables obligatorias definidas
- Formato de variables correcto
- Contraseña SQL Server cumple requisitos de complejidad
- Puertos válidos (1024-65535)

---

## Script Principal de Desarrollo

### ../run-local.ps1 (en la raíz del proyecto)
Script recomendado para desarrollo local que automatiza todo el proceso.

**Uso:**
```powershell
.\run-local.ps1                    # Inicio completo
.\run-local.ps1 -Clean             # Limpieza + inicio
.\run-local.ps1 -NoBuild           # Sin reconstruir imágenes
.\run-local.ps1 -Foreground        # Ver logs en tiempo real
```

**Flujo automático:**
1. Verifica Docker ejecutándose
2. Inicializa .env desde .env.example si no existe
3. Valida configuración
4. Limpia contenedores anteriores (con -Clean)
5. Construye imágenes Docker
6. Inicia servicios en modo detached
7. Espera health checks (max 2 minutos)
8. Muestra URLs de acceso

**Este es el script recomendado para iniciar el desarrollo diario.**

---

## Ejemplos de Uso

### Inicio rápido para desarrollo
```powershell
.\run-local.ps1
```

### Inicio limpio (resetear todo)
```powershell
.\run-local.ps1 -Clean
```

### Ver logs en tiempo real
```powershell
.\run-local.ps1 -Foreground
```

### Validar configuración sin iniciar
```powershell
.\scripts\validate-env.ps1
```

---

## Datos de Prueba

El sistema carga automáticamente datos de prueba al iniciar en modo desarrollo:

- **Tenant Demo**: Tenant de prueba para desarrollo
- **Usuario Admin**: `admin1@backoffice.com` / `Admin123!`

Los datos se cargan desde el código C# (`DatabaseSeeder.cs` en Infrastructure) cuando `SEED_DATABASE=true`.

Para usar la API con el tenant, incluye el header en tus peticiones:
```
X-Tenant-Id: 1
```

Ejemplo con curl:
```bash
curl -H "X-Tenant-Id: 1" http://localhost:5000/api/users
```

---

**Documentación completa**: `docs/DOCKER.md`
