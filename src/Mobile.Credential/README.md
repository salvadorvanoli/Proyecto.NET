# Mobile.Credential - Credencial Digital

Aplicación móvil Android para emulación de credenciales digitales mediante tecnología NFC HCE (Host Card Emulation).

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
- Soporte para **Host Card Emulation (HCE)** - Android 4.4 (API 19) o superior
- Cable USB para depuración

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
# b1d454120123    device
```

### 3. Configurar Backend

La aplicación requiere acceso al backend API. Verificar la configuración en:

```
Mobile.Credential/Services/AuthService.cs
```

Actualizar la IP del servidor si es necesario:
```csharp
private const string BaseUrl = "http://192.168.1.23:5000";
```

**Importante**: El dispositivo debe poder acceder a esta IP (misma red WiFi que el servidor).

## 🔨 Compilación

### Desde la Terminal

```powershell
# Navegar al directorio del proyecto
cd c:\Nadia\.NET\Proyecto.NET\src\Mobile.Credential

# Compilar para Android (arm64)
dotnet build -f net8.0-android -p:RuntimeIdentifier=android-arm64

# El APK se genera en:
# bin\Debug\net8.0-android\android-arm64\com.companyname.credential-Signed.apk
```

### Desde Visual Studio 2022

1. Abrir `Proyecto.NET.sln`
2. Seleccionar **Mobile.Credential** como proyecto de inicio
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
adb install -r com.companyname.credential-Signed.apk

# Si hay múltiples dispositivos:
adb -s b1d454120123 install -r com.companyname.credential-Signed.apk
```

### Opción 2: Desde Visual Studio

Simplemente presionar **F5** con el dispositivo seleccionado.

## 🧪 Uso de la Aplicación

### Login

Credenciales de prueba:
- **Email**: `admin1@backoffice.com`
- **Password**: `Admin123!`

### Activar Credencial NFC

1. Iniciar sesión en la aplicación
2. Navegar a la pantalla **Credencial**
3. Tocar el botón **"Activar Credencial NFC"**
4. La aplicación emulará la credencial mediante HCE
5. Acercar el dispositivo a un lector NFC (punto de control)

### Verificación de Funcionalidad

```powershell
# Ver logs en tiempo real
adb logcat | Select-String "HCE|NFC|Credential"

# Verificar que el servicio HCE está registrado
adb shell dumpsys nfc | Select-String "HCE"
```

## 🔧 Solución de Problemas

### Error: "No se puede conectar al backend"

- Verificar que el backend está corriendo: `http://192.168.1.23:5000/health`
- Comprobar que el dispositivo está en la misma red WiFi
- Revisar la IP en `AuthService.cs`

### Error: "NFC no disponible"

- Verificar que el dispositivo tiene NFC
- Habilitar NFC en: **Configuración** → **Conexiones** → **NFC**

### Error al compilar: "Java SDK not found"

```powershell
# Verificar JAVA_HOME
$env:JAVA_HOME

# Si no está configurado, establecerlo:
[System.Environment]::SetEnvironmentVariable('JAVA_HOME', 'C:\Program Files\Microsoft\jdk-17.x.x', 'Machine')
```

### Error: "Android SDK not found"

Verificar `ANDROID_HOME` o instalar desde Visual Studio Installer.

## 📚 Arquitectura

- **Patrón**: MVVM (Model-View-ViewModel)
- **Inyección de Dependencias**: Microsoft.Extensions.DependencyInjection
- **NFC HCE**: Android HostApduService
- **Navegación**: MAUI Shell

### Estructura de Carpetas

```
Mobile.Credential/
├── Pages/              # Vistas XAML
│   ├── LoginPage.xaml
│   └── CredentialPage.xaml
├── ViewModels/         # Lógica de presentación
│   ├── LoginViewModel.cs
│   └── CredentialViewModel.cs
├── Services/           # Servicios de aplicación
│   ├── AuthService.cs
│   └── INfcCredentialService.cs
├── Platforms/Android/  # Código específico de Android
│   └── Services/
│       └── NfcHostCardEmulationService.cs
└── Models/             # Modelos de datos
    └── LoginResponse.cs
```

## 🔗 Referencias del Proyecto

- **Shared**: Proyecto con DTOs compartidos
- Sin dependencias a Domain o Application (solo credenciales)

## 👥 Equipo

- Nadia Gorría
- Joaquín Jozami
- Salvador Vanoli
- Valentín Veintemilla

---

Para más información sobre el sistema completo, consultar el [README principal](../../README.md).
