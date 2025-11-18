# Mejoras de Seguridad - FrontOffice

## Resumen de Cambios

Se ha adaptado el sistema de autenticación y seguridad del FrontOffice para seguir las mismas prácticas implementadas en Web.API y Web.BackOffice, aplicando principios de Clean Architecture y mejores prácticas de seguridad.

## Componentes Creados

### 1. **JwtTokenHandler.cs**
`src/Web.FrontOffice/Services/JwtTokenHandler.cs`

**Propósito**: DelegatingHandler que agrega automáticamente el token JWT y el header TenantId a todas las peticiones HTTP salientes.

**Características**:
- Intercepta todas las peticiones HTTP antes de enviarlas
- Extrae el token JWT del estado de autenticación
- Agrega el header `Authorization: Bearer {token}`
- Agrega el header `X-Tenant-Id` para multi-tenancy
- Logging detallado para auditoría y debugging

**Beneficios de Seguridad**:
- ✅ Token JWT adjuntado automáticamente en cada request
- ✅ No es necesario agregar manualmente headers en cada llamada
- ✅ Previene olvidos de autenticación en endpoints protegidos
- ✅ Logging de todas las peticiones autenticadas

### 2. **CustomAuthenticationStateProvider.cs**
`src/Web.FrontOffice/Services/CustomAuthenticationStateProvider.cs`

**Propósito**: Gestiona el estado de autenticación del usuario en Blazor Server usando ProtectedBrowserStorage.

**Características**:
- Almacenamiento seguro de sesión en localStorage cifrado
- Validación de expiración de tokens
- Gestión de claims de usuario (UserId, Email, Roles, TenantId)
- Notificación automática de cambios de estado de autenticación
- Métodos helper para obtener TenantId y UserId

**Beneficios de Seguridad**:
- ✅ Almacenamiento cifrado de datos sensibles
- ✅ Validación automática de tokens expirados
- ✅ Claims tipados y seguros
- ✅ Separación de concerns (autenticación vs UI)

### 3. **Login.razor (Refactorizado)**
`src/Web.FrontOffice/Components/Pages/Login.razor`

**Mejoras Implementadas**:
- Uso de `CustomAuthenticationStateProvider` en lugar de localStorage directo
- Validación robusta de respuesta del servidor
- Manejo de errores específico (HTTP, validación, inesperados)
- Logging estructurado de intentos de login
- Soporte para ReturnUrl (redirección después del login)
- Verificación de presencia de token en la respuesta

**Beneficios de Seguridad**:
- ✅ No almacena tokens en localStorage sin cifrar
- ✅ Validación de respuesta del servidor
- ✅ Logging de intentos de autenticación (auditoría)
- ✅ Manejo seguro de redirecciones

### 4. **Logout.razor**
`src/Web.FrontOffice/Components/Pages/Logout.razor`

**Características**:
- Página dedicada para cierre de sesión
- Limpieza completa del estado de autenticación
- Feedback visual durante el proceso
- Redirección automática al login

**Beneficios de Seguridad**:
- ✅ Cierre de sesión centralizado
- ✅ Limpieza completa de datos de sesión
- ✅ Prevención de sesiones zombies

## Componentes Actualizados

### 5. **Program.cs**
Configuración de autenticación y autorización:

```csharp
// Autenticación y Autorización
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Custom Authentication State Provider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// JWT Token Handler
builder.Services.AddScoped<JwtTokenHandler>();
```

**HttpClients configurados con JwtTokenHandler**:
- INewsApiService
- INotificationApiService
- IUserApiService
- IBenefitApiService
- IAccessEventApiService
- ITenantApiService

### 6. **UserMenu.razor**
- Migrado de ProtectedLocalStorage a AuthenticationStateProvider
- Uso de componente `<AuthorizeView>`
- Navegación a `/logout` en lugar de limpieza manual

### 7. **MainLayout.razor**
- Migrado de ProtectedLocalStorage a AuthenticationStateProvider
- Obtención de TenantId desde claims en lugar de localStorage

### 8. **NotificationBell.razor**
- Migrado de ProtectedLocalStorage a AuthenticationStateProvider
- Extracción de UserId desde claims

### 9. **NavMenu.razor**
- Migrado de ProtectedLocalStorage a AuthenticationStateProvider
- Extracción de TenantId desde claims

## Arquitectura de Seguridad

### Flujo de Autenticación

```
1. Usuario ingresa credenciales → Login.razor
2. Login.razor → AuthApiService.LoginAsync()
3. AuthApiService → Web.API /api/auth/login
4. Web.API valida credenciales y genera JWT
5. Web.API devuelve LoginResponse con token JWT
6. Login.razor → CustomAuthenticationStateProvider.MarkUserAsAuthenticatedAsync()
7. CustomAuthenticationStateProvider guarda sesión cifrada en ProtectedLocalStorage
8. CustomAuthenticationStateProvider notifica cambio de estado
9. Blazor actualiza todos los componentes con <AuthorizeView>
```

### Flujo de Petición Autenticada

```
1. Componente Blazor → API Service (ej: BenefitApiService)
2. HttpClient interceptado por JwtTokenHandler
3. JwtTokenHandler obtiene AuthenticationState
4. JwtTokenHandler agrega headers:
   - Authorization: Bearer {jwt_token}
   - X-Tenant-Id: {tenant_id}
5. Petición enviada a Web.API
6. Web.API valida JWT y TenantId
7. Web.API procesa request y devuelve respuesta
```

## Principios de Clean Architecture Aplicados

### 1. **Separación de Concerns**
- **Services**: Lógica de autenticación (CustomAuthenticationStateProvider)
- **Handlers**: Interceptación de HTTP (JwtTokenHandler)
- **Components**: UI y presentación (Login.razor, Logout.razor)

### 2. **Dependency Injection**
- Todos los servicios registrados en DI container
- Scoped lifetime apropiado para Blazor Server
- Fácil testeo y mocking

### 3. **Single Responsibility**
- `JwtTokenHandler`: Solo agrega headers de autenticación
- `CustomAuthenticationStateProvider`: Solo gestiona estado de autenticación
- `Login.razor`: Solo maneja UI y validación de login

### 4. **Encapsulación**
- Detalles de almacenamiento ocultos en CustomAuthenticationStateProvider
- Componentes no acceden directamente a ProtectedLocalStorage
- Claims management centralizado

## Mejores Prácticas de Seguridad Implementadas

### ✅ **Autenticación y Autorización**
- JWT Bearer Token Authentication
- Claims-based authorization
- Role-based access control (preparado para uso)
- Token expiration validation

### ✅ **Multi-Tenancy**
- TenantId en claims
- Header X-Tenant-Id en todas las peticiones
- Validación de tenant en backend

### ✅ **Almacenamiento Seguro**
- ProtectedBrowserStorage (cifrado por ASP.NET Core Data Protection)
- No almacenamiento de contraseñas en cliente
- Tokens almacenados de forma segura

### ✅ **Logging y Auditoría**
- Logging de intentos de login exitosos y fallidos
- Logging de peticiones autenticadas
- Warnings para casos anómalos

### ✅ **Manejo de Errores**
- Mensajes de error genéricos al usuario (no revelan detalles)
- Logging detallado en backend para diagnóstico
- Try-catch apropiados en operaciones críticas

### ✅ **Validación**
- Validación de entrada en formularios (DataAnnotations)
- Validación de respuestas del servidor
- Verificación de tokens antes de almacenar

## Compatibilidad con BackOffice y Web.API

La implementación del FrontOffice ahora es consistente con:

### **Web.BackOffice**
- ✅ Mismo patrón de JwtTokenHandler
- ✅ Mismos DTOs (LoginRequest, LoginResponse)
- ✅ Mismo flujo de autenticación
- ✅ Mismos headers HTTP

### **Web.API**
- ✅ Compatible con JWT authentication
- ✅ Compatible con X-Tenant-Id header
- ✅ Compatible con rate limiting por IP
- ✅ Compatible con CORS policy

## Próximos Pasos Recomendados

### 1. **Autorización basada en roles**
```csharp
// En components que requieran roles específicos
<AuthorizeView Roles="Estudiante,Docente">
    <Authorized>
        // Contenido solo para usuarios autorizados
    </Authorized>
    <NotAuthorized>
        // Mensaje de acceso denegado
    </NotAuthorized>
</AuthorizeView>
```

### 2. **Refresh Token**
- Implementar refresh token para renovación automática
- Evitar logout forzado cuando expire el token

### 3. **Redirección automática al login**
- Middleware para detectar 401 Unauthorized
- Redirigir a /login con ReturnUrl

### 4. **Remember Me**
- Actualmente el checkbox existe pero no se usa
- Implementar persistencia extendida de sesión

### 5. **Two-Factor Authentication (2FA)**
- Agregar segundo factor de autenticación
- SMS, Email, o Authenticator App

## Testing

### Puntos de Testing Recomendados

1. **CustomAuthenticationStateProvider**
   - Login exitoso
   - Login con credenciales inválidas
   - Token expirado
   - Logout

2. **JwtTokenHandler**
   - Headers agregados correctamente
   - Comportamiento con usuario no autenticado
   - Logging apropiado

3. **Login.razor**
   - Validación de formulario
   - Manejo de errores
   - Redirección después de login

4. **Integration Tests**
   - Flujo completo de login
   - Peticiones autenticadas a API
   - Multi-tenancy

## Conclusión

El FrontOffice ahora tiene un sistema de autenticación robusto, seguro y alineado con las mejores prácticas de:
- ✅ Clean Architecture
- ✅ SOLID principles
- ✅ Security best practices
- ✅ ASP.NET Core patterns
- ✅ Blazor Server patterns

La implementación es consistente con Web.BackOffice y Web.API, facilitando el mantenimiento y la evolución del sistema.
