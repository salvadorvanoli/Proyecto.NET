# Guía Rápida - Sistema de Autenticación FrontOffice

## 🔐 Autenticación de Usuario

### Login
```csharp
// El usuario ingresa sus credenciales en /login
// El sistema automáticamente:
// 1. Valida las credenciales contra Web.API
// 2. Almacena el JWT token de forma segura (cifrado)
// 3. Actualiza el estado de autenticación
// 4. Redirige al usuario a la página solicitada
```

### Logout
```csharp
// Navegar a /logout
// El sistema automáticamente:
// 1. Limpia el estado de autenticación
// 2. Elimina datos de sesión cifrados
// 3. Redirige al login
```

## 🛡️ Protección de Componentes

### Opción 1: AuthorizeView (Recomendado)
```razor
<AuthorizeView>
    <Authorized>
        <p>Bienvenido, @context.User.Identity.Name!</p>
    </Authorized>
    <NotAuthorized>
        <p>Debes iniciar sesión.</p>
    </NotAuthorized>
</AuthorizeView>
```

### Opción 2: Por Rol
```razor
<AuthorizeView Roles="Estudiante,Docente">
    <Authorized>
        <p>Contenido solo para estudiantes y docentes</p>
    </Authorized>
    <NotAuthorized>
        <p>No tienes permisos para ver este contenido</p>
    </NotAuthorized>
</AuthorizeView>
```

### Opción 3: Programática
```razor
@inject AuthenticationStateProvider AuthStateProvider

@code {
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (!user.Identity?.IsAuthenticated ?? false)
        {
            Navigation.NavigateTo("/login");
            return;
        }
        
        // Continuar con la lógica
    }
}
```

## 📡 Llamadas a API Autenticadas

### NO es necesario agregar headers manualmente
```csharp
// ❌ INCORRECTO - No hacer esto
var response = await httpClient.GetAsync("/api/benefits", new HttpRequestMessage
{
    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
});

// ✅ CORRECTO - Solo llamar al servicio
var benefits = await BenefitApiService.GetUserBenefitsAsync(userId);
// JwtTokenHandler automáticamente agrega el token y TenantId
```

### Todos los servicios configurados con JwtTokenHandler
- ✅ INewsApiService
- ✅ INotificationApiService
- ✅ IUserApiService
- ✅ IBenefitApiService
- ✅ IAccessEventApiService
- ✅ ITenantApiService

## 🔑 Acceso a Claims del Usuario

### Obtener UserId
```csharp
@inject CustomAuthenticationStateProvider AuthStateProvider

@code {
    private async Task<int?> GetCurrentUserId()
    {
        return await AuthStateProvider.GetUserIdAsync();
    }
}
```

### Obtener TenantId
```csharp
@inject CustomAuthenticationStateProvider AuthStateProvider

@code {
    private async Task<int?> GetCurrentTenantId()
    {
        return await AuthStateProvider.GetTenantIdAsync();
    }
}
```

### Obtener Claims completos
```csharp
@inject AuthenticationStateProvider AuthStateProvider

@code {
    private async Task GetUserClaims()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var name = user.FindFirst(ClaimTypes.Name)?.Value;
        var tenantId = user.FindFirst("TenantId")?.Value;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}
```

## 🔧 Casos de Uso Comunes

### 1. Componente que requiere autenticación
```razor
@page "/my-profile"
@attribute [Authorize] 
@* Esto redirige automáticamente a /login si no está autenticado *@

<h3>Mi Perfil</h3>
```

### 2. Mostrar nombre del usuario en la UI
```razor
<AuthorizeView>
    <Authorized>
        <p>Hola, @context.User.Identity.Name</p>
    </Authorized>
</AuthorizeView>
```

### 3. Cargar datos del usuario al iniciar componente
```razor
@inject CustomAuthenticationStateProvider AuthStateProvider
@inject IUserApiService UserApiService

@code {
    private UserDto? currentUser;

    protected override async Task OnInitializedAsync()
    {
        var userId = await AuthStateProvider.GetUserIdAsync();
        if (userId.HasValue)
        {
            currentUser = await UserApiService.GetUserByIdAsync(userId.Value);
        }
    }
}
```

### 4. Verificar si el usuario tiene un rol específico
```razor
@inject AuthenticationStateProvider AuthStateProvider

@code {
    private async Task<bool> IsAdmin()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        return authState.User.IsInRole("Administrador");
    }
}
```

## 🚨 Manejo de Errores

### Token Expirado
El `CustomAuthenticationStateProvider` automáticamente detecta tokens expirados y marca al usuario como no autenticado.

### Error de Conexión
```csharp
try
{
    var data = await ApiService.GetDataAsync();
}
catch (HttpRequestException ex)
{
    Logger.LogError(ex, "Error de conexión con el API");
    errorMessage = "No se pudo conectar con el servidor";
}
```

### No Autorizado (401)
Si el API devuelve 401, es porque:
1. El token no está presente
2. El token es inválido
3. El token ha expirado

**Solución**: Redirigir al usuario al login
```csharp
if (response.StatusCode == HttpStatusCode.Unauthorized)
{
    Navigation.NavigateTo("/login", forceLoad: true);
}
```

## 📝 Buenas Prácticas

### ✅ DO
- Usar `<AuthorizeView>` para mostrar/ocultar contenido según autenticación
- Inyectar `CustomAuthenticationStateProvider` cuando necesites info del usuario
- Dejar que `JwtTokenHandler` maneje los headers de autenticación
- Hacer logout navegando a `/logout`
- Usar logging para auditoría de autenticación

### ❌ DON'T
- No acceder directamente a `ProtectedLocalStorage` para datos de sesión
- No crear tu propio sistema de autenticación
- No almacenar tokens en variables de JavaScript
- No agregar manualmente headers de autenticación
- No confiar solo en la UI para seguridad (siempre validar en backend)

## 🔄 Flujo Completo de Autenticación

```
Usuario → /login
  ↓
Ingresa credenciales
  ↓
Login.razor → AuthApiService.LoginAsync()
  ↓
Web.API valida y devuelve JWT
  ↓
CustomAuthenticationStateProvider guarda sesión cifrada
  ↓
AuthenticationState actualizado
  ↓
Blazor re-renderiza componentes
  ↓
Usuario redirigido a la página solicitada
  ↓
[Usuario autenticado]
  ↓
Peticiones a API → JwtTokenHandler agrega headers
  ↓
Web.API valida JWT
  ↓
Respuesta devuelta a componente
```

## 🧪 Testing

### Test de Login
```csharp
// Probar con credenciales válidas
Email: usuario@test.com
Password: Password123!

// Verificar:
// ✅ Redirección exitosa
// ✅ Token almacenado
// ✅ Estado de autenticación actualizado
```

### Test de Petición Autenticada
```csharp
// 1. Hacer login
// 2. Llamar a un endpoint protegido
// 3. Verificar que el header Authorization esté presente
// 4. Verificar que el header X-Tenant-Id esté presente
```

### Test de Logout
```csharp
// 1. Hacer login
// 2. Navegar a /logout
// 3. Verificar que se limpia el localStorage
// 4. Verificar que se redirige a /login
// 5. Intentar acceder a página protegida (debe redirigir a login)
```

## 📚 Referencias

- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Blazor Authentication](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [OWASP Security](https://owasp.org/www-project-top-ten/)
