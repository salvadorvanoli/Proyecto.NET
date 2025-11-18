# Medidas de Seguridad Implementadas

## ✅ Resumen General

El sistema **Proyecto.NET** utiliza **ASP.NET Core 8.0** e implementa las siguientes medidas de seguridad:

---

## 🔒 1. TLS/HTTPS

### Estado: ✅ Implementado
- **Backend API**: `UseHttpsRedirection()` activo
- **FrontOffice**: `UseHttpsRedirection()` activo  
- **BackOffice**: `UseHttpsRedirection()` activo

### Configuración:
```csharp
app.UseHttpsRedirection();
```

### HSTS (HTTP Strict Transport Security):
```csharp
if (!app.Environment.IsDevelopment())
{
    context.Response.Headers.Append("Strict-Transport-Security", 
        "max-age=31536000; includeSubDomains");
}
```

### ⚠️ Notas:
- En desarrollo se usa HTTP para facilitar pruebas locales
- En producción debe configurarse un certificado TLS válido
- HSTS solo se activa en entornos no-desarrollo

---

## 🚦 2. Rate Limiting

### Estado: ✅ Implementado en todas las aplicaciones

#### Backend API (`Web.Api`):
```csharp
// Límites configurados:
- POST /api/auth/login: 5 requests/minuto por IP (protección brute-force)
- Endpoints generales: 10 requests/segundo por IP
- Endpoints generales: 200 requests/minuto por IP
```

#### FrontOffice (`Web.FrontOffice`):
```csharp
// Límites configurados:
- POST /api/auth/login: 5 requests/minuto por IP
- POST /api/auth/*: 10 requests/minuto por IP
- Endpoints generales: 50 requests/10 segundos (ajustado para Blazor SignalR)
- Endpoints generales: 200 requests/minuto por IP
```

### Biblioteca utilizada:
- **AspNetCoreRateLimit v5.0.0**
- Almacenamiento en memoria (MemoryCache)
- HTTP 429 (Too Many Requests) cuando se excede el límite

### Código:
```csharp
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.HttpStatusCode = 429;
    // ... configuración de reglas
});

// Middleware
app.UseIpRateLimiting();
```

---

## 🌐 3. CORS (Cross-Origin Resource Sharing)

### Estado: ✅ Restringido en Backend API

#### Backend API:
```csharp
// Desarrollo - Orígenes específicos permitidos:
- http://localhost:5001 (BackOffice)
- http://localhost:5002 (FrontOffice)  
- http://localhost:5000 (Otros)

// Producción - Orígenes configurables vía:
1. Variable de entorno: CORS_ALLOWED_ORIGINS (separado por comas)
2. appsettings.json: Cors:AllowedOrigins (array)

// Configuración segura:
policy
    .WithOrigins(allowedOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
```

#### FrontOffice/BackOffice:
- **No aplica CORS** - Son aplicaciones servidor que renderizan HTML
- No reciben requests cross-origin directamente

---

## 🛡️ 4. Security Headers

### Estado: ✅ Implementados en todas las aplicaciones

Todos los proyectos (API, FrontOffice, BackOffice) configuran los siguientes headers:

### Headers Configurados:

#### 1. **HSTS** (HTTP Strict Transport Security)
```http
Strict-Transport-Security: max-age=31536000; includeSubDomains
```
- Fuerza HTTPS por 1 año
- Solo en producción

#### 2. **X-Frame-Options**
```http
X-Frame-Options: DENY (API) / SAMEORIGIN (FrontOffice/BackOffice)
```
- **DENY**: No permite iframe en absoluto (API)
- **SAMEORIGIN**: Permite iframe solo del mismo origen (Apps Blazor)
- Protege contra clickjacking

#### 3. **X-Content-Type-Options**
```http
X-Content-Type-Options: nosniff
```
- Previene MIME type sniffing
- Navegador respeta el Content-Type declarado

#### 4. **X-XSS-Protection**
```http
X-XSS-Protection: 1; mode=block
```
- Activa filtro XSS del navegador
- Bloquea la página si detecta XSS

#### 5. **Content-Security-Policy (CSP)**

**Backend API:**
```http
Content-Security-Policy: default-src 'self'; 
  script-src 'self'; 
  style-src 'self' 'unsafe-inline'; 
  img-src 'self' data: https:; 
  font-src 'self'; 
  connect-src 'self'; 
  frame-ancestors 'none'
```

**FrontOffice/BackOffice (ajustado para Blazor):**
```http
Content-Security-Policy: default-src 'self'; 
  script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; 
  style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; 
  img-src 'self' data: https:; 
  font-src 'self' https://cdn.jsdelivr.net; 
  connect-src 'self' ws: wss:; 
  frame-ancestors 'self'
```

⚠️ **Nota sobre Blazor:** 
- `'unsafe-inline'` y `'unsafe-eval'` son necesarios para Blazor Server
- `ws:` y `wss:` son necesarios para SignalR WebSockets
- Esto reduce la efectividad de CSP pero es un requisito de Blazor

#### 6. **Referrer-Policy**
```http
Referrer-Policy: strict-origin-when-cross-origin
```
- Solo envía origin en requests cross-origin HTTPS
- Protege información de URLs

#### 7. **Permissions-Policy**
```http
Permissions-Policy: geolocation=(), microphone=(), camera=()
```
- Deshabilita acceso a geolocalización, micrófono y cámara
- Reduce superficie de ataque

---

## 🔐 5. Autenticación y Autorización

### Backend API:
- **JWT (JSON Web Tokens)** con firma HMACSHA256
- Tokens con expiración de 8 horas
- Claims: UserId, Email, FullName, TenantId, Roles
- Validación de token en cada request

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // ... configuración
        };
    });
```

### FrontOffice/BackOffice:
- **Cookie Authentication** (ASP.NET Core Identity Cookies)
- Cookies HttpOnly (protección XSS)
- Cookies Secure en producción
- SameSite=Lax (protección CSRF)
- Expiración: 8 horas con sliding expiration

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
```

---

## 🛠️ 6. Otras Medidas de Seguridad

### Antiforgery (CSRF Protection):
```csharp
builder.Services.AddAntiforgery();
app.UseAntiforgery();
```
- Protección automática contra CSRF en formularios Blazor
- Tokens de validación en cada POST

### Exception Handling:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
```
- Manejo global de excepciones
- No expone detalles técnicos en producción

### Logging y Observabilidad:
- Logs estructurados con ILogger
- CorrelationId para tracing de requests
- Health checks para monitoreo

---

## 📋 Checklist de Seguridad

| Medida | Backend API | FrontOffice | BackOffice | Estado |
|--------|-------------|-------------|------------|--------|
| TLS/HTTPS | ✅ | ✅ | ✅ | Implementado |
| Rate Limiting | ✅ | ✅ | ⚠️ | API y Front OK |
| CORS Restringido | ✅ | N/A | N/A | Implementado |
| Security Headers | ✅ | ✅ | ✅ | Implementado |
| Autenticación JWT/Cookie | ✅ | ✅ | ✅ | Implementado |
| Autorización por Roles | ✅ | ✅ | ✅ | Implementado |
| Antiforgery (CSRF) | ✅ | ✅ | ✅ | Implementado |
| Passwords Hasheados | ✅ | N/A | N/A | BCrypt usado |
| Input Validation | ✅ | ✅ | ✅ | FluentValidation |
| SQL Injection Protection | ✅ | N/A | N/A | EF Core (parametrizado) |

---

## 🚀 Recomendaciones Adicionales

### Para Producción:

1. **Certificado TLS Válido**
   - Obtener certificado de Let's Encrypt o proveedor comercial
   - Configurar en servidor web (IIS, Kestrel, nginx)

2. **Base de Datos**
   - Usar conexiones encriptadas (SSL/TLS)
   - Credenciales en Azure Key Vault o AWS Secrets Manager
   - Nunca en código fuente

3. **Secrets Management**
   - Usar Azure Key Vault, AWS Secrets Manager o HashiCorp Vault
   - Variables de entorno para configuración sensible
   - Nunca commitear appsettings.Production.json con secrets

4. **Rate Limiting Distribuido**
   - Cambiar de MemoryCache a Redis para multi-instancia
   - Usar Azure API Management o AWS API Gateway

5. **WAF (Web Application Firewall)**
   - CloudFlare, Azure Front Door, o AWS WAF
   - Protección adicional contra OWASP Top 10

6. **Monitoreo de Seguridad**
   - Application Insights / CloudWatch
   - Alertas de intentos de login fallidos
   - Detección de patrones de ataque

7. **Auditoría**
   - Logs de todas las operaciones sensibles
   - Retención de logs por compliance
   - SIEM para análisis de seguridad

---

## 📚 Referencias

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Content Security Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit)

---

**Última actualización:** Noviembre 18, 2025
