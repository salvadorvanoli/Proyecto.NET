# Configurar Conexión al Backend desde Mobile

## Problema
Por defecto, `localhost` en un dispositivo Android se refiere **al propio dispositivo**, no a tu PC. Por eso la app no puede conectarse al backend que corre en tu computadora.

## Solución

### 1. Obtener la IP de tu PC en la red local

**En Windows (PowerShell o CMD):**
```powershell
ipconfig
```
Busca la sección "Adaptador de LAN inalámbrica Wi-Fi" o "Adaptador Ethernet" y anota la **Dirección IPv4**.

Ejemplo: `192.168.1.100`

**En Linux/Mac:**
```bash
ifconfig
# o
ip addr show
```

### 2. Configurar la IP en MauiProgram.cs

Abre `src/Mobile/MauiProgram.cs` y reemplaza `localhost` con tu IP:

```csharp
builder.Services.AddHttpClient<IAccessEventApiService, AccessEventApiService>(client =>
{
    // REEMPLAZAR con la IP de tu PC
    client.BaseAddress = new Uri("http://192.168.1.100:5000");  // ⬅️ AQUÍ
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
});
```

### 3. Asegurarse que el backend acepta conexiones externas

El backend debe escuchar en todas las interfaces, no solo `localhost`.

Verifica en `src/Web.Api/Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://0.0.0.0:5000"  // ⬅️ 0.0.0.0 permite conexiones externas
    }
  }
}
```

O inicia el backend así:
```powershell
cd src/Web.Api
dotnet run --urls "http://0.0.0.0:5000"
```

### 4. Verificar firewall

Asegúrate de que el firewall de Windows permita conexiones en el puerto 5000:

```powershell
# PowerShell como Administrador
New-NetFirewallRule -DisplayName "ASP.NET Core Web API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

### 5. Verificar conectividad

Antes de probar la app:

1. **PC y smartphone deben estar en la misma red WiFi**
2. Desde el navegador del smartphone, ve a: `http://192.168.1.100:5000/api/accessevents`
3. Si ves una respuesta JSON (aunque sea vacía `[]`), la conexión funciona

### 6. Recompilar e instalar la app

```powershell
cd src/Mobile
dotnet build -f net8.0-android -c Debug
adb install -r bin/Debug/net8.0-android/com.companyname.mobile-Signed.apk
```

## Verificar logs

Si hay problemas de conexión, revisa los logs de la app:

```powershell
adb logcat | Select-String "Mobile|AccessEvent|HTTP"
```

## Resumen de Flujo

```
[Smartphone] ──NFC Tag──> [MAUI App]
                             │
                             │ HTTP POST /api/accessevents
                             │
                             ▼
                          [PC: Web.Api:5000]
                             │
                             │ Entity Framework
                             │
                             ▼
                          [SQL Server]
```

## Troubleshooting

**Error: "No se pudo conectar con el servidor"**
- ✅ Verifica que backend esté corriendo: `http://tu-ip:5000/api/accessevents` desde el navegador del smartphone
- ✅ Verifica firewall de Windows
- ✅ Confirma que PC y smartphone están en la misma red WiFi
- ✅ Verifica que la IP en `MauiProgram.cs` sea correcta

**Error: "Connection refused"**
- ✅ El backend debe escuchar en `0.0.0.0:5000`, no en `localhost:5000`
