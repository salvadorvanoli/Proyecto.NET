# Despliegue en Dispositivo Real

## Requisitos Previos

### Para Android
1. **Habilitar Modo Desarrollador:**
   - Ve a `Configuración > Acerca del teléfono`
   - Toca 7 veces en `Número de compilación`
   - Se habilitará el modo desarrollador

2. **Habilitar Depuración USB:**
   - Ve a `Configuración > Opciones de desarrollador`
   - Activa `Depuración USB`
   - Activa `Instalar vía USB` (si está disponible)

3. **Conectar dispositivo:**
   - Conecta el teléfono via USB a tu PC
   - Acepta el mensaje de confianza en el teléfono

### Para iOS
1. **Cuenta de desarrollador Apple** (gratuita o de pago)
2. **Xcode instalado** en Mac
3. **Certificado de desarrollador configurado**

## Verificar Dispositivo Conectado

### Android
```powershell
# Desde la carpeta Mobile
cd c:\Nadia\.NET\Proyecto.NET\src\Mobile

# Verificar dispositivos Android conectados
dotnet build -t:AdbDevices
```

Si ves tu dispositivo listado, está correctamente conectado.

## Compilar y Desplegar

### Opción 1: Despliegue Debug (Recomendado para desarrollo)
```powershell
# Compilar e instalar en dispositivo Android conectado
dotnet build -t:Run -f net8.0-android

# O específicamente instalar
dotnet build -t:Install -f net8.0-android
```

### Opción 2: Generar APK para Instalación Manual
```powershell
# Generar APK debug
dotnet publish -f net8.0-android -c Debug

# El APK estará en:
# bin\Debug\net8.0-android\publish\
```

### Opción 3: Generar APK Release (Para distribución)
```powershell
# Generar APK release firmado
dotnet publish -f net8.0-android -c Release
```

## Configurar Backend URL

**IMPORTANTE:** Debes configurar la URL del backend para que el dispositivo real pueda conectarse.

### Si el backend está en tu PC local:

1. **Obtener IP local de tu PC:**
```powershell
ipconfig
# Buscar IPv4 Address de tu red WiFi (ejemplo: 192.168.1.100)
```

2. **Asegurar que el dispositivo esté en la misma red WiFi**

3. **Actualizar URL del backend:**
   - Edita `Mobile/MauiProgram.cs`
   - Cambia `http://localhost:5000` por `http://TU_IP:5000`
   - Ejemplo: `http://192.168.1.100:5000`

4. **Permitir conexiones externas en el backend:**
```powershell
# En Web.Api/Properties/launchSettings.json
# Cambiar "applicationUrl": "http://localhost:5000"
# Por: "applicationUrl": "http://0.0.0.0:5000"
```

### Si el backend está desplegado en AWS/Cloud:
- Usa la URL pública del ALB o servicio
- Ejemplo: `https://tu-dominio.com` o `http://tu-alb-12345.us-east-1.elb.amazonaws.com`

## Probar NFC en Dispositivo Real

### 1. Verificar soporte NFC:
```csharp
// El código ya incluye verificación
// Ver AccessNfcViewModel.cs - CheckNfcSupport()
```

### 2. Habilitar NFC en el dispositivo:
- Ve a `Configuración > Conexiones inalámbricas > NFC`
- Activa NFC
- Activa `Android Beam` (si está disponible)

### 3. Probar lectura:
- Abre la app
- Ve a la página de Acceso NFC
- Acerca una tarjeta NFC al lector (parte trasera del teléfono)
- Debe aparecer el UID y validarse el acceso

## Pruebas de Funcionalidad Offline

### 1. Modo Avión:
```
1. Conecta el dispositivo y despliega la app
2. Activa Modo Avión (o desactiva WiFi/Datos móviles)
3. Escanea una tarjeta NFC
4. Verifica que aparezca "● Offline" en la UI
5. Verifica que el evento se guarde localmente
6. Desactiva Modo Avión
7. Debe aparecer "● Online" y sincronizar automáticamente
```

### 2. Verificar Sincronización Manual:
```
1. Con eventos pendientes offline
2. Restaura conectividad
3. Toca el botón "Sincronizar Ahora"
4. El badge debe mostrar 0 eventos pendientes
```

### 3. Verificar Base de Datos SQLite:
```powershell
# Extraer base de datos del dispositivo
adb pull /data/data/com.companyname.mobile/files/localaccess.db3 ./

# Abrir con cualquier herramienta SQLite (DB Browser for SQLite, etc.)
```

## Logs y Debugging

### Ver logs en tiempo real:
```powershell
# Android logcat filtrado
adb logcat | Select-String "Mobile"

# O más específico
adb logcat | Select-String "AccessNfc|SyncService"
```

### Debugging con Visual Studio Code:
1. Conecta el dispositivo via USB
2. Presiona F5 o `Run > Start Debugging`
3. Selecciona el dispositivo de la lista
4. Coloca breakpoints en el código
5. La app se desplegará y pausará en los breakpoints

## Solución de Problemas

### El dispositivo no se detecta:
```powershell
# Reiniciar ADB
adb kill-server
adb start-server
adb devices
```

### Error de firma/certificado:
```powershell
# Limpiar y reconstruir
dotnet clean
dotnet build -t:Run -f net8.0-android
```

### NFC no funciona:
1. Verificar que el dispositivo tenga NFC (no todos lo tienen)
2. Verificar que NFC esté habilitado en configuración
3. Verificar permisos en `AndroidManifest.xml`
4. Probar con diferentes posiciones de la tarjeta

### Backend no accesible:
1. Verificar que estén en la misma red WiFi
2. Verificar firewall de Windows
3. Verificar que el backend escuche en `0.0.0.0` no `localhost`
4. Probar conectividad: `http://TU_IP:5000/api/health` desde navegador del móvil

## Permisos Android

Verificar en `Platforms/Android/AndroidManifest.xml`:
```xml
<uses-permission android:name="android.permission.NFC" />
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

## Performance

Para mejor rendimiento en dispositivo real:
```powershell
# Compilar en modo Release con AOT
dotnet publish -f net8.0-android -c Release /p:RunAOTCompilation=true
```

## Distribución

### Google Play Store:
1. Generar APK/AAB firmado con certificado de producción
2. Crear cuenta de desarrollador Google Play ($25 único)
3. Subir a Play Console

### Distribución Interna:
- Compartir APK directamente
- Usar Firebase App Distribution
- Usar AppCenter de Microsoft
