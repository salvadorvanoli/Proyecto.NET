# Mobile.AccessPoint - Punto de Control

Aplicación móvil Android para puntos de control de acceso mediante lectura NFC de credenciales digitales.

## 📋 Requisitos Previos

### Software Necesario

1. **.NET 8 SDK**
   ```powershell
   # Verificar instalación
   dotnet --version  # Debe mostrar 8.x.x
   ```
   Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0

2. **MAUI Workload**
   ```powershell
   # Instalar workload de MAUI
   dotnet workload install maui

   # Verificar instalación
   dotnet workload list
   ```

3. **Android SDK** (API Level 21 o superior)
   - Puedes instalarlo desde Visual Studio Installer → Individual Components → Android SDK
   - O mediante Android Studio: https://developer.android.com/studio

4. **JDK 17** (requerido para compilar Android)
   - Recomendado: Microsoft Build of OpenJDK
   - Descargar desde: https://learn.microsoft.com/java/openjdk/download

5. **Android Debug Bridge (adb)**
   - Incluido en Android SDK platform-tools
   - Verificar: `adb version`

### Hardware Necesario

- Dispositivo Android con **NFC** habilitado
- Soporte para lectura NFC - Android 2.3 (API 9) o superior
- Cable USB para depuración
- **Conexión de red estable** (la aplicación requiere conectividad al backend)

## 🚀 Configuración Inicial

### 1. Habilitar Depuración USB en el Dispositivo

1. Abrir **Configuración** → **Acerca del teléfono**
2. Tocar **Número de compilación** 7 veces para habilitar opciones de desarrollador
3. Volver a **Configuración** → **Sistema** → **Opciones de desarrollador**
4. Activar **Depuración USB**

### 2. Conectar Dispositivo

```powershell
# Conectar el teléfono via USB
# Verificar que adb detecta el dispositivo
adb devices

# Debería mostrar algo como:
# List of devices attached
# bedac672    device
```

### 3. Configurar Backend

⚠️ **IMPORTANTE**: Esta aplicación **SIEMPRE requiere conexión al backend** para funcionar. No tiene modo offline.

Verificar la configuración en los servicios API:

```
Mobile.AccessPoint/Services/AuthService.cs
Mobile.AccessPoint/Services/AccessEventApiService.cs
Mobile.AccessPoint/Services/AccessRuleApiService.cs
```

Actualizar la IP del servidor si es necesario:
```csharp
private const string BaseUrl = "http://192.168.1.23:5000";
```

**Requisitos de Red**:
- El dispositivo debe estar en la misma red WiFi que el servidor
- El puerto 5000 debe ser accesible
- Probar conectividad: `http://192.168.1.23:5000/health`

### 4. Iniciar el Backend

```powershell
# Navegar al directorio del backend
cd c:\Nadia\.NET\Proyecto.NET\src\Web.Api

# Iniciar el servidor
dotnet run

# O en una ventana separada:
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'c:\Nadia\.NET\Proyecto.NET\src\Web.Api'; dotnet run"

# Verificar que está corriendo
Invoke-WebRequest http://192.168.1.23:5000/health
```

## 🔨 Compilación

### Desde la Terminal

```powershell
# Navegar al directorio del proyecto
cd c:\Nadia\.NET\Proyecto.NET\src\Mobile.AccessPoint

# Compilar para Android (arm64)
dotnet build -f net8.0-android -p:RuntimeIdentifier=android-arm64

# El APK se genera en:
# bin\Debug\net8.0-android\android-arm64\com.companyname.accesspoint-Signed.apk
```

### Desde Visual Studio 2022

1. Abrir `Proyecto.NET.sln`
2. Seleccionar **Mobile.AccessPoint** como proyecto de inicio
3. En la barra de herramientas:
   - Target Framework: `net8.0-android`
   - Configuration: `Debug`
   - Device: Seleccionar tu dispositivo conectado
4. Presionar **F5** o hacer clic en **Run**

## 📱 Instalación en Dispositivo

### Opción 1: Instalación Manual

```powershell
# Navegar al directorio del APK
cd bin\Debug\net8.0-android\android-arm64

# Instalar en el dispositivo conectado
adb install -r com.companyname.accesspoint-Signed.apk

# Si hay múltiples dispositivos:
adb -s bedac672 install -r com.companyname.accesspoint-Signed.apk
```

### Opción 2: Desde Visual Studio

Simplemente presionar **F5** con el dispositivo seleccionado.

## 🧪 Uso de la Aplicación

### Login

Credenciales de prueba:
- **Email**: `admin1@backoffice.com` o `admin11@backoffice.com`
- **Password**: `Admin123!`

### Leer Credencial NFC

1. Iniciar sesión en la aplicación
2. La aplicación mostrará la pantalla de lectura NFC
3. Acercar un dispositivo con credencial NFC activa
4. La aplicación:
   - Lee el UID de la credencial
   - Valida contra el backend
   - Muestra resultado (Acceso permitido/denegado) con retroalimentación visual y vibración
   - Registra el evento de acceso

### Verificación de Funcionalidad

```powershell
# Ver logs en tiempo real
adb logcat | Select-String "NFC|Access|Validation"

# Filtrar solo logs de la aplicación
adb logcat | Select-String "Mobile.AccessPoint"
```

## 🔧 Solución de Problemas

### Error: "El punto de control requiere conexión al backend para funcionar"

- Verificar que el backend está corriendo:
  ```powershell
  Invoke-WebRequest http://192.168.1.23:5000/health
  ```
- Comprobar que el dispositivo está en la misma red WiFi
- Revisar la IP en los servicios API (`*ApiService.cs`)
- Verificar firewall del servidor

### Error: "NFC no disponible"

- Verificar que el dispositivo tiene NFC
- Habilitar NFC en: **Configuración** → **Conexiones** → **NFC**
- Reiniciar la aplicación después de habilitar NFC

### La aplicación vibra pero no detecta la credencial

- Verificar que `MainActivity.cs` tiene el método `OnNewIntent` implementado
- Revisar logs con: `adb logcat | Select-String "OnNewIntent|ProcessIntent"`
- Asegurarse de que el servicio NFC está registrado en `MauiProgram.cs`

### Error al compilar: "Java SDK not found"

```powershell
# Verificar JAVA_HOME
$env:JAVA_HOME

# Si no está configurado, establecerlo:
[System.Environment]::SetEnvironmentVariable('JAVA_HOME', 'C:\Program Files\Microsoft\jdk-17.x.x', 'Machine')
```

### Error: "Android SDK not found"

Verificar `ANDROID_HOME` o instalar desde Visual Studio Installer.

### Advertencia: "CA1416 - Platform compatibility"

Esto es normal - el código Android solo se ejecuta en Android. Puede ignorarse.

## 📚 Arquitectura

- **Patrón**: MVVM (Model-View-ViewModel)
- **Inyección de Dependencias**: Microsoft.Extensions.DependencyInjection
- **NFC Reader**: Android NfcAdapter con filtros ISO-DEP y NDEF
- **Navegación**: MAUI Shell
- **Validación**: Siempre online contra backend API

### Estructura de Carpetas

```
Mobile.AccessPoint/
├── Pages/              # Vistas XAML
│   ├── LoginPage.xaml
│   └── AccessNfcPage.xaml
├── ViewModels/         # Lógica de presentación
│   ├── LoginViewModel.cs
│   └── AccessNfcViewModel.cs
├── Services/           # Servicios de aplicación
│   ├── AuthService.cs
│   ├── AccessEventApiService.cs
│   ├── AccessRuleApiService.cs
│   ├── AccessRuleService.cs     # Online-only validation
│   └── NfcService.cs            # Partial class
├── Platforms/Android/  # Código específico de Android
│   ├── MainActivity.cs          # OnNewIntent para NFC
│   └── NfcServiceAndroid.cs     # Partial implementation
└── Models/             # Modelos de datos
    └── LoginResponse.cs
```

### Diferencia con Mobile.Credential

- **Mobile.AccessPoint**: Lee credenciales NFC (lector)
- **Mobile.Credential**: Emula credenciales NFC (tarjeta)
- AccessPoint siempre valida online, no tiene base de datos local
- AccessPoint usa ISO-DEP y NDEF, Credential usa HCE

## 🔗 Referencias del Proyecto

- **Shared**: DTOs compartidos
- **Domain**: Entidades y reglas de dominio
- **Application**: Lógica de aplicación y casos de uso

## ⚙️ Configuración de Red

La aplicación usa `network_security_config.xml` para permitir tráfico HTTP cleartext (desarrollo):

```xml
<!-- Platforms/Android/Resources/xml/network_security_config.xml -->
<network-security-config>
    <base-config cleartextTrafficPermitted="true">
        <trust-anchors>
            <certificates src="system" />
        </trust-anchors>
    </base-config>
</network-security-config>
```

**Producción**: Cambiar a HTTPS y actualizar la configuración de seguridad.

## 👥 Equipo

- Nadia Gorría
- Joaquín Jozami
- Salvador Vanoli
- Valentín Veintemilla

---

Para más información sobre el sistema completo, consultar el [README principal](../../README.md).
