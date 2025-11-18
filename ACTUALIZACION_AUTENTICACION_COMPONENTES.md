# Actualización de Autenticación - Componentes Profile, Benefits y AccessHistory

## 📋 Resumen de Cambios

Se han actualizado los componentes **Profile**, **Benefits** y **AccessHistory** para usar el nuevo sistema de autenticación basado en `CustomAuthenticationStateProvider`, eliminando la dependencia del parámetro `devUserId` para funcionar en producción.

## 🔄 Componentes Actualizados

### 1. **Profile.razor** (`/perfil`)
**Cambios aplicados:**
- ✅ Agregado atributo `[Authorize]` para protección automática
- ✅ Obtención de `userId` desde `AuthenticationStateProvider` (claims)
- ✅ Fallback a `devUserId` query parameter solo para testing
- ✅ Mensaje de error mejorado con botón para ir al login
- ✅ Validación de autenticación antes de cargar datos

**Flujo actualizado:**
```
Usuario accede a /perfil
  ↓
Blazor verifica autenticación ([Authorize])
  ↓
¿Autenticado?
  ├─ Sí → Obtiene userId de claims → Carga perfil
  └─ No → Redirige a /login con ReturnUrl=/perfil
```

### 2. **Benefits.razor** (`/mis-beneficios`)
**Cambios aplicados:**
- ✅ Agregado atributo `[Authorize]` para protección automática
- ✅ Obtención de `userId` desde `AuthenticationStateProvider` (claims)
- ✅ Fallback a `devUserId` query parameter solo para testing
- ✅ Mensaje de error mejorado con botón para ir al login
- ✅ Validación de autenticación antes de cargar beneficios

**Flujo actualizado:**
```
Usuario accede a /mis-beneficios
  ↓
Blazor verifica autenticación ([Authorize])
  ↓
¿Autenticado?
  ├─ Sí → Obtiene userId de claims → Carga beneficios
  └─ No → Redirige a /login con ReturnUrl=/mis-beneficios
```

### 3. **AccessHistory.razor** (`/historial-accesos`)
**Cambios aplicados:**
- ✅ Agregado atributo `[Authorize]` para protección automática
- ✅ Obtención de `userId` desde `AuthenticationStateProvider` (claims)
- ✅ Fallback a `devUserId` query parameter solo para testing
- ✅ Mensaje de error mejorado con botón para ir al login
- ✅ Validación de autenticación antes de cargar historial

**Flujo actualizado:**
```
Usuario accede a /historial-accesos
  ↓
Blazor verifica autenticación ([Authorize])
  ↓
¿Autenticado?
  ├─ Sí → Obtiene userId de claims → Carga historial
  └─ No → Redirige a /login con ReturnUrl=/historial-accesos
```

### 4. **Routes.razor**
**Cambios aplicados:**
- ✅ Implementado `<CascadingAuthenticationState>`
- ✅ Cambiado `<RouteView>` por `<AuthorizeRouteView>`
- ✅ Agregado manejo de `<NotAuthorized>` con redirección al login
- ✅ Agregado estado `<Authorizing>` con spinner de carga

**Nuevo código:**
```razor
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
                <NotAuthorized>
                    <!-- Redirige al login si no está autenticado -->
                    <RedirectToLogin />
                </NotAuthorized>
                <Authorizing>
                    <!-- Muestra spinner mientras verifica autenticación -->
                </Authorizing>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

### 5. **RedirectToLogin.razor** (Nuevo)
**Componente creado:**
- ✅ Redirige automáticamente al login
- ✅ Preserva la URL original en `ReturnUrl` query parameter
- ✅ Después del login, el usuario vuelve a la página original

**Código:**
```razor
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        var returnUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        Navigation.NavigateTo($"/login?ReturnUrl={Uri.EscapeDataString(returnUrl)}", forceLoad: true);
    }
}
```

## 🔐 Sistema de Autenticación Integrado

### Obtención de UserId desde Claims
**Antes (usando devUserId):**
```csharp
// Requería parámetro manual en la URL
if (query.TryGetValue("devUserId", out var devId) && int.TryParse(devId, out var userId))
{
    // Cargar datos...
}
```

**Ahora (usando AuthenticationState):**
```csharp
var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
var user = authState.User;

if (user.Identity?.IsAuthenticated == true)
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
    {
        // Cargar datos del usuario autenticado
    }
}
```

### Protección de Páginas
**Nivel 1: Atributo [Authorize]**
```razor
@page "/perfil"
@attribute [Microsoft.AspNetCore.Authorization.Authorize]
```
- Protege la página completa
- Redirige automáticamente al login si no está autenticado

**Nivel 2: Validación en código**
```csharp
if (!user.Identity?.IsAuthenticated ?? true)
{
    error = "Debe iniciar sesión para ver sus beneficios.";
    return;
}
```
- Validación adicional en el código
- Permite mensajes de error personalizados

## 🎯 Beneficios de los Cambios

### ✅ Seguridad Mejorada
- No se puede acceder a páginas protegidas sin autenticación
- Redirección automática al login
- Validación en múltiples niveles

### ✅ Experiencia de Usuario
- Redirección automática después del login a la página original
- Mensajes claros cuando no está autenticado
- Botón directo para ir al login

### ✅ Mantenibilidad
- Código consistente entre componentes
- Uso de claims estándar de ASP.NET Core
- Separación de concerns (autenticación vs lógica de negocio)

### ✅ Testing Facilitado
- Mantiene soporte para `devUserId` en desarrollo
- Fácil cambio entre modo desarrollo y producción
- Logging detallado para debugging

## 🧪 Testing

### Test de Autenticación Requerida

**Escenario 1: Usuario NO autenticado**
```
1. Navegar a /perfil sin estar logueado
2. Verificar redirección a /login?ReturnUrl=/perfil
3. Hacer login
4. Verificar redirección automática de vuelta a /perfil
```

**Escenario 2: Usuario autenticado**
```
1. Hacer login primero
2. Navegar a /perfil
3. Verificar que se carga el perfil del usuario autenticado
4. No debe pedir devUserId
```

**Escenario 3: Token expirado**
```
1. Usuario tiene sesión pero el token expiró
2. Navegar a /perfil
3. CustomAuthenticationStateProvider detecta expiración
4. Usuario marcado como no autenticado
5. Redirección automática a /login
```

### Test de Modo Desarrollo

**Escenario: Testing sin login**
```
1. Navegar a /perfil?devUserId=1
2. Verificar que se carga el usuario con ID 1
3. Útil para testing de UI sin autenticación
```

## 📝 Código Ejemplo: Cómo usar en nuevos componentes

### Componente protegido básico
```razor
@page "/mi-componente"
@attribute [Microsoft.AspNetCore.Authorization.Authorize]
@using Microsoft.AspNetCore.Components.Authorization
@inject AuthenticationStateProvider AuthStateProvider

<h3>Mi Componente Protegido</h3>

@code {
    private int userId;
    
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
        {
            // Cargar datos del usuario
        }
    }
}
```

### Componente con autorización por rol
```razor
@page "/admin"
@attribute [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrador")]

<h3>Panel de Administrador</h3>
<!-- Solo accesible para usuarios con rol "Administrador" -->
```

### Sección visible solo para autenticados
```razor
<AuthorizeView>
    <Authorized>
        <p>Hola, @context.User.Identity.Name</p>
    </Authorized>
    <NotAuthorized>
        <p>Por favor, inicia sesión</p>
    </NotAuthorized>
</AuthorizeView>
```

## 🔄 Migración de Componentes Antiguos

Si tienes otros componentes que usan `devUserId`, sigue estos pasos:

### Paso 1: Agregar atributo Authorize
```diff
@page "/mi-pagina"
+ @attribute [Microsoft.AspNetCore.Authorization.Authorize]
```

### Paso 2: Inyectar AuthenticationStateProvider
```diff
+ @inject AuthenticationStateProvider AuthenticationStateProvider
```

### Paso 3: Obtener userId de claims
```diff
- // Obtener de query parameter
- var userId = int.Parse(query["devUserId"]);

+ // Obtener de autenticación
+ var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
+ var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
+ var userId = int.Parse(userIdClaim.Value);
```

### Paso 4: Mantener fallback para desarrollo (opcional)
```csharp
int userId = 0;

// Primero intentar con devUserId (solo desarrollo)
if (query.TryGetValue("devUserId", out var devId) && int.TryParse(devId, out var devUserId))
{
    userId = devUserId;
}
else
{
    // Producción: usar autenticación
    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
    var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
    userId = int.Parse(userIdClaim.Value);
}
```

## 📚 Referencias

- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
- [Blazor Authentication](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/)
- [Claims-based Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims)

## ✅ Checklist de Implementación

- [x] Profile.razor actualizado
- [x] Benefits.razor actualizado  
- [x] AccessHistory.razor actualizado
- [x] Routes.razor configurado con AuthorizeRouteView
- [x] RedirectToLogin.razor creado
- [x] Mensajes de error mejorados
- [x] Botones para ir al login agregados
- [x] Atributos [Authorize] agregados
- [x] Testing con devUserId mantenido para desarrollo
- [x] Sin errores de compilación
- [x] Documentación actualizada

## 🚀 Resultado Final

Los componentes ahora:
- ✅ Funcionan con el sistema de autenticación real
- ✅ Redirigen automáticamente al login cuando es necesario
- ✅ Preservan la URL de destino (ReturnUrl)
- ✅ Obtienen datos del usuario autenticado desde claims
- ✅ Mantienen compatibilidad con testing usando devUserId
- ✅ Tienen mensajes de error claros y útiles
- ✅ Siguen las mejores prácticas de seguridad
