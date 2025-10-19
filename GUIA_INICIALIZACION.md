# Guía de Inicialización de la Base de Datos y Uso del Endpoint de Usuarios

## 📋 Resumen de lo Implementado

He creado un sistema completo para registrar usuarios en tu aplicación multi-tenant con las siguientes capas:

### ✅ Archivos Creados

**Application Layer:**
- `Application/Common/Interfaces/IApplicationDbContext.cs` - Interfaz para el DbContext
- `Application/Common/Interfaces/ITenantProvider.cs` - Interfaz para el proveedor de tenant
- `Application/Common/Interfaces/IPasswordHasher.cs` - Interfaz para hasheo de contraseñas
- `Application/Users/DTOs/CreateUserRequest.cs` - DTO para crear usuarios
- `Application/Users/DTOs/UserResponse.cs` - DTO de respuesta de usuario
- `Application/Users/Services/IUserService.cs` - Interfaz del servicio de usuarios
- `Application/Users/Services/UserService.cs` - Implementación del servicio
- `Application/DependencyInjection.cs` - Registro de servicios

**Infrastructure Layer:**
- `Infrastructure/Services/TenantProvider.cs` - Implementación del proveedor de tenant
- `Infrastructure/Services/PasswordHasher.cs` - Implementación de hasheo de contraseñas (PBKDF2)
- `Infrastructure/DependencyInjection.cs` - Actualizado con nuevos servicios

**Web.Api Layer:**
- `Web.Api/Controllers/UsersController.cs` - Controller con endpoints para usuarios
- `Web.Api/Program.cs` - Actualizado con configuración completa
- `Web.Api/appsettings.json` - Actualizado con cadena de conexión

## 🚀 Pasos para Inicializar la Base de Datos

### 1. Crear la Migración Inicial

Abre una terminal (CMD o PowerShell) en la raíz del proyecto y ejecuta:

```bash
dotnet ef migrations add InitialCreate --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

### 2. Aplicar la Migración a la Base de Datos

```bash
dotnet ef database update --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

Esto creará la base de datos `ProyectoNetDb` en tu SQL Server LocalDB con todas las tablas necesarias.

### 3. Insertar un Tenant de Prueba

Antes de poder crear usuarios, necesitas tener al menos un tenant en la base de datos. Ejecuta este SQL:

```sql
USE ProyectoNetDb;
GO

INSERT INTO Tenants (Name, CreatedAt, UpdatedAt)
VALUES ('Tenant Demo', GETUTCDATE(), GETUTCDATE());
GO

-- Ver el ID del tenant creado
SELECT * FROM Tenants;
```

Anota el `Id` del tenant que se creó (probablemente será 1).

## 🎯 Cómo Usar el Endpoint

### 1. Iniciar la API

```bash
cd src\Web.Api
dotnet run
```

La API se ejecutará en `https://localhost:5001` o `http://localhost:5000`.

### 2. Crear un Usuario

**Endpoint:** `POST /api/users`

**Headers Requeridos:**
- `Content-Type: application/json`
- `X-Tenant-Id: 1` (usa el ID del tenant que creaste)

**Body (JSON):**
```json
{
  "email": "usuario@ejemplo.com",
  "password": "MiPassword123!",
  "firstName": "Juan",
  "lastName": "Pérez",
  "dateOfBirth": "1990-01-15T00:00:00"
}
```

**Respuesta Exitosa (201 Created):**
```json
{
  "id": 1,
  "email": "usuario@ejemplo.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "fullName": "Juan Pérez",
  "dateOfBirth": "1990-01-15T00:00:00",
  "tenantId": 1,
  "createdAt": "2025-10-19T12:30:00Z",
  "updatedAt": "2025-10-19T12:30:00Z"
}
```

### 3. Obtener un Usuario por ID

**Endpoint:** `GET /api/users/{id}`

**Headers Requeridos:**
- `X-Tenant-Id: 1`

### 4. Obtener un Usuario por Email

**Endpoint:** `GET /api/users/by-email/{email}`

**Headers Requeridos:**
- `X-Tenant-Id: 1`

### 5. Obtener Todos los Usuarios del Tenant Actual

**Endpoint:** `GET /api/users`

**Headers Requeridos:**
- `X-Tenant-Id: 1`

**Descripción:** Devuelve todos los usuarios que pertenecen al tenant especificado en el header.

### 6. Obtener Todos los Usuarios (Todos los Tenants)

**Endpoint:** `GET /api/users/all`

**Headers Requeridos:**
- Ninguno (no requiere X-Tenant-Id)

**Descripción:** Devuelve **TODOS** los usuarios de **TODOS** los tenants. Útil para operaciones de administración del sistema.

⚠️ **IMPORTANTE:** Este endpoint debe ser protegido en producción con autorización de administrador.

## 🧪 Ejemplos con cURL

### Crear Usuario:
```bash
curl -X POST https://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 1" \
  -d "{\"email\":\"usuario@ejemplo.com\",\"password\":\"MiPassword123!\",\"firstName\":\"Juan\",\"lastName\":\"Pérez\",\"dateOfBirth\":\"1990-01-15T00:00:00\"}" \
  -k
```

### Obtener Usuario por ID:
```bash
curl -X GET https://localhost:5001/api/users/1 \
  -H "X-Tenant-Id: 1" \
  -k
```

### Obtener Usuario por Email:
```bash
curl -X GET https://localhost:5001/api/users/by-email/usuario@ejemplo.com \
  -H "X-Tenant-Id: 1" \
  -k
```

### Obtener Todos los Usuarios del Tenant Actual:
```bash
curl -X GET https://localhost:5001/api/users \
  -H "X-Tenant-Id: 1" \
  -k
```

### Obtener Todos los Usuarios (Todos los Tenants):
```bash
curl -X GET https://localhost:5001/api/users/all \
  -k
```

## 📝 Ejemplos con Swagger

1. Navega a `https://localhost:5001/swagger`
2. Haz clic en "POST /api/users"
3. Haz clic en "Try it out"
4. En el campo de headers, agrega: `X-Tenant-Id: 1`
5. Completa el body con los datos del usuario
6. Haz clic en "Execute"

## 🔐 Características de Seguridad Implementadas

- **Multi-tenancy:** Todos los usuarios están aislados por tenant mediante el header `X-Tenant-Id`
- **Hash de Contraseñas:** Las contraseñas se hashean usando PBKDF2 con sal aleatoria (10,000 iteraciones)
- **Validación de Email:** Se valida el formato del email y se convierte a minúsculas
- **Validación de Datos Personales:** Se validan nombres y fecha de nacimiento según las reglas del dominio
- **Prevención de Duplicados:** No se permite crear usuarios con el mismo email en un tenant

## ⚠️ Notas Importantes

1. **Tenant ID Obligatorio:** Siempre debes enviar el header `X-Tenant-Id` en todas las peticiones
2. **Contraseñas:** Las contraseñas se hashean automáticamente, nunca se almacenan en texto plano
3. **Validaciones:** La fecha de nacimiento debe estar en el pasado y cumplir con las restricciones de edad del dominio
4. **Email Único:** Cada email debe ser único dentro de un tenant (pero puede repetirse en diferentes tenants)

## 🐛 Solución de Problemas

### Error: "Tenant ID is not set"
- Asegúrate de enviar el header `X-Tenant-Id` con un valor numérico válido

### Error: "Tenant with ID X does not exist"
- Verifica que el tenant existe en la base de datos ejecutando: `SELECT * FROM Tenants`

### Error: "User with email 'X' already exists in this tenant"
- El email ya está registrado en ese tenant, usa otro email o verifica el usuario existente

### La base de datos no se crea
- Verifica que SQL Server LocalDB esté instalado y corriendo
- Prueba la conexión con SQL Server Management Studio o Azure Data Studio
- Verifica la cadena de conexión en `appsettings.json`

## 📊 Estructura de la Base de Datos

El sistema creará las siguientes tablas principales:
- `Tenants` - Inquilinos del sistema
- `Users` - Usuarios con datos personales
- `Credentials` - Credenciales digitales
- `Roles` - Roles de usuario
- `Spaces` - Espacios físicos
- `AccessEvents` - Eventos de acceso
- Y más...

## 🎉 ¡Listo!

Ya tienes todo configurado para empezar a crear usuarios en tu sistema multi-tenant. Si tienes algún problema, revisa los logs de la aplicación o verifica que todos los pasos se hayan ejecutado correctamente.
