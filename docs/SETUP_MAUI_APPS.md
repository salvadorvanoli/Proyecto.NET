# 📱 Guía de Setup - Aplicaciones Móviles MAUI

Guía completa para configurar el entorno de desarrollo de las aplicaciones móviles **Mobile.Credential** y **Mobile.AccessPoint**.

## 🎯 Objetivo

Permitir a cualquier miembro del equipo compilar, desplegar y probar las aplicaciones móviles en sus propios dispositivos Android.

---

## 📋 Requisitos del Sistema

### Sistema Operativo
- ✅ **Windows 10/11** (64-bit)
- ✅ **macOS** (con Visual Studio for Mac)
- ✅ **Linux** (con VS Code y .NET CLI)

### Hardware
- **Espacio en disco**: ~15 GB (SDK + Android SDK + workloads)
- **RAM**: Mínimo 8 GB, recomendado 16 GB
- **Procesador**: 64-bit, multi-core

---

## 🛠️ Instalación de Herramientas

### 1. .NET 8 SDK

**Verificar instalación existente:**
```powershell
dotnet --version
# Debe mostrar 8.x.x (ej: 8.0.404)
```

**Instalar si no está disponible:**
1. Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0
2. Ejecutar el instalador
3. Reiniciar terminal
4. Verificar con `dotnet --version`

---

### 2. MAUI Workload

**Instalar el workload de .NET MAUI:**
```powershell
# Instalar MAUI
dotnet workload install maui

# Verificar instalación
dotnet workload list
# Debe aparecer: maui
```

**Actualizar workloads (si ya estaban instalados):**
```powershell
dotnet workload update
```

---

### 3. Java Development Kit (JDK) 17

**Verificar instalación:**
```powershell
java -version
# Debe mostrar: openjdk version "17.x.x"
```

**Instalar Microsoft Build of OpenJDK 17:**
1. Descargar desde: https://learn.microsoft.com/java/openjdk/download
2. Ejecutar instalador
3. **Configurar variable de entorno:**
   ```powershell
   # PowerShell como administrador
   [System.Environment]::SetEnvironmentVariable('JAVA_HOME', 'C:\Program Files\Microsoft\jdk-17.0.x.x-hotspot', 'Machine')
   ```
4. Reiniciar terminal y verificar `java -version`

---

### 4. Android SDK

#### Opción A: Visual Studio Installer (Windows - Recomendado)

1. Abrir **Visual Studio Installer**
2. Modificar instalación existente
3. Ir a **Individual components**
4. Buscar y seleccionar:
   - ✅ Android SDK Setup (API Level 34)
   - ✅ Android SDK Build-Tools
   - ✅ Android Emulator (opcional)
5. Aplicar cambios

#### Opción B: Android Studio (Multiplataforma)

1. Descargar Android Studio: https://developer.android.com/studio
2. Ejecutar instalador
3. Abrir Android Studio → **More Actions** → **SDK Manager**
4. En **SDK Platforms**, seleccionar:
   - ✅ Android 14.0 (API Level 34)
   - ✅ Android 10.0 (API Level 29) - para compatibilidad
5. En **SDK Tools**, seleccionar:
   - ✅ Android SDK Build-Tools 34.0.0
   - ✅ Android SDK Platform-Tools
   - ✅ Android SDK Command-line Tools
   - ✅ NDK (Side by side)
6. Aplicar cambios

**Configurar variable de entorno:**
```powershell
# Windows - PowerShell como administrador
[System.Environment]::SetEnvironmentVariable('ANDROID_HOME', 'C:\Users\<TuUsuario>\AppData\Local\Android\Sdk', 'User')

# O para Visual Studio:
[System.Environment]::SetEnvironmentVariable('ANDROID_HOME', 'C:\Program Files (x86)\Android\android-sdk', 'Machine')
```

**Verificar:**
```powershell
adb version
# Android Debug Bridge version x.x.x
```

---

### 5. Visual Studio 2022 (Opcional pero Recomendado - Windows)

**Instalación completa:**
1. Descargar Visual Studio 2022 Community/Professional: https://visualstudio.microsoft.com/downloads/
2. Durante instalación, seleccionar workloads:
   - ✅ **.NET Multi-platform App UI development** (MAUI)
   - ✅ **ASP.NET and web development** (para backend)
   - ✅ **Mobile development with .NET**
3. En **Individual components**, verificar:
   - ✅ .NET 8.0 Runtime
   - ✅ Android SDK Setup
   - ✅ Java SDK

**Extensiones recomendadas:**
- XAML Styler
- ReSharper (opcional)

---

### 6. Visual Studio Code (Alternativa multiplataforma)

**Instalación:**
1. Descargar: https://code.visualstudio.com/
2. Instalar extensiones:
   - ✅ **C# Dev Kit** (Microsoft)
   - ✅ **.NET MAUI** (Microsoft)
   - ✅ **Android iOS Emulator** (opcional)

---

## 📱 Configuración del Dispositivo Android

### Habilitar Modo Desarrollador

1. Abrir **Configuración** en el dispositivo
2. Ir a **Acerca del teléfono** (o About phone)
3. Tocar **Número de compilación** 7 veces consecutivas
4. Verás mensaje: "Ahora eres desarrollador"

### Habilitar Depuración USB

1. Volver a **Configuración**
2. Ir a **Sistema** → **Opciones de desarrollador** (Developer options)
3. Activar **Depuración USB** (USB debugging)
4. Activar **Instalar via USB** (Install via USB)

### Habilitar NFC

**Para Mobile.Credential y Mobile.AccessPoint:**
1. Ir a **Configuración** → **Conexiones** (o Wireless & networks)
2. Activar **NFC**
3. Verificar que NFC está habilitado en la barra de notificaciones

---

## 🔌 Conectar Dispositivo

### Conectar via USB

1. Conectar el dispositivo Android al PC con cable USB
2. En el dispositivo, autorizar la conexión:
   - Aparecerá un popup: **"¿Permitir depuración USB?"**
   - Marcar **"Permitir siempre desde esta computadora"**
   - Tocar **Permitir**

### Verificar Conexión

```powershell
# Listar dispositivos conectados
adb devices

# Salida esperada:
# List of devices attached
# bedac672    device
# b1d454120123    device
```

**Si aparece "unauthorized":**
- Desconectar y reconectar cable
- Verificar autorización en el dispositivo
- Ejecutar: `adb kill-server` y luego `adb devices`

---

## 🏗️ Clonar el Repositorio

```powershell
# Clonar repo
git clone https://github.com/tu-organizacion/Proyecto.NET.git
cd Proyecto.NET

# Cambiar a la rama de trabajo (ej: Nadia)
git checkout Nadia
```

---

## 🔧 Compilar las Aplicaciones

### Desde la Terminal

#### Mobile.Credential (Credencial Digital)

```powershell
# Navegar al proyecto
cd src\Mobile.Credential

# Restaurar dependencias
dotnet restore

# Compilar para Android (arm64)
dotnet build -f net8.0-android -p:RuntimeIdentifier=android-arm64

# El APK se genera en:
# bin\Debug\net8.0-android\android-arm64\com.companyname.credential-Signed.apk
```

#### Mobile.AccessPoint (Punto de Control)

```powershell
# Navegar al proyecto
cd ..\Mobile.AccessPoint

# Restaurar dependencias
dotnet restore

# Compilar para Android (arm64)
dotnet build -f net8.0-android -p:RuntimeIdentifier=android-arm64

# El APK se genera en:
# bin\Debug\net8.0-android\android-arm64\com.companyname.accesspoint-Signed.apk
```

### Desde Visual Studio 2022

1. Abrir `Proyecto.NET.sln`
2. En **Solution Explorer**, clic derecho en el proyecto (Mobile.Credential o Mobile.AccessPoint)
3. Seleccionar **Set as Startup Project**
4. En la barra de herramientas:
   - **Target Framework**: `net8.0-android`
   - **Configuration**: `Debug` (o `Release`)
   - **Device**: Seleccionar tu dispositivo conectado
5. Presionar **F5** o clic en **Play** (▶️)

---

## 📦 Instalar en Dispositivo

### Instalación Manual (adb)

```powershell
# Mobile.Credential
cd bin\Debug\net8.0-android\android-arm64
adb install -r com.companyname.credential-Signed.apk

# Mobile.AccessPoint
cd ..\..\..\..\Mobile.AccessPoint\bin\Debug\net8.0-android\android-arm64
adb install -r com.companyname.accesspoint-Signed.apk

# Si tienes múltiples dispositivos:
adb -s bedac672 install -r com.companyname.accesspoint-Signed.apk
adb -s b1d454120123 install -r com.companyname.credential-Signed.apk
```

### Desde Visual Studio

Presionar **F5** con el dispositivo seleccionado. Visual Studio automáticamente:
- Compila el proyecto
- Instala el APK
- Inicia la aplicación en el dispositivo

---

## 🌐 Configurar Conexión al Backend

### 1. Iniciar Backend Localmente

```powershell
# Navegar al proyecto API
cd c:\Nadia\.NET\Proyecto.NET\src\Web.Api

# Iniciar servidor
dotnet run

# El servidor estará disponible en: http://localhost:5000
```

### 2. Encontrar tu IP Local

```powershell
# Windows
ipconfig

# Buscar: "Dirección IPv4" en tu adaptador WiFi o Ethernet
# Ejemplo: 192.168.1.23
```

### 3. Actualizar IP en las Aplicaciones

**Mobile.Credential:**
Editar `src/Mobile.Credential/Services/AuthService.cs`:
```csharp
private const string BaseUrl = "http://<TU_IP_LOCAL>:5000";
// Ejemplo: http://192.168.1.23:5000
```

**Mobile.AccessPoint:**
Editar los siguientes archivos:
- `src/Mobile.AccessPoint/Services/AuthService.cs`
- `src/Mobile.AccessPoint/Services/AccessEventApiService.cs`
- `src/Mobile.AccessPoint/Services/AccessRuleApiService.cs`

```csharp
private const string BaseUrl = "http://<TU_IP_LOCAL>:5000";
```

### 4. Verificar Conectividad

**Desde tu PC:**
```powershell
Invoke-WebRequest http://<TU_IP_LOCAL>:5000/health
# Debería retornar: StatusCode 200
```

**Importante**: 
- El dispositivo Android debe estar en la **misma red WiFi** que tu PC
- Firewall de Windows debe permitir conexiones en el puerto 5000
- No usar `localhost` o `127.0.0.1` desde el dispositivo móvil

---

## 🧪 Probar las Aplicaciones

### Mobile.Credential (Dispositivo B - Emisor)

1. **Login:**
   - Email: `admin1@backoffice.com`
   - Password: `Admin123!`

2. **Activar Credencial:**
   - Ir a **Credencial**
   - Tocar **"Activar Credencial NFC"**
   - La credencial se emula via HCE

### Mobile.AccessPoint (Dispositivo A - Lector)

1. **Login:**
   - Email: `admin1@backoffice.com`
   - Password: `Admin123!`

2. **Leer Credencial:**
   - La app muestra pantalla de lectura NFC
   - Acercar Dispositivo B (con credencial activa)
   - La app lee, valida y muestra resultado

---

## 🐛 Solución de Problemas Comunes

### Error: "Java SDK not found"

```powershell
# Verificar JAVA_HOME
$env:JAVA_HOME

# Si está vacío, configurar:
[System.Environment]::SetEnvironmentVariable('JAVA_HOME', 'C:\Program Files\Microsoft\jdk-17.0.x.x-hotspot', 'Machine')

# Reiniciar terminal
```

### Error: "Android SDK not found"

```powershell
# Verificar ANDROID_HOME
$env:ANDROID_HOME

# Configurar si es necesario:
[System.Environment]::SetEnvironmentVariable('ANDROID_HOME', 'C:\Users\<TuUsuario>\AppData\Local\Android\Sdk', 'User')

# Reiniciar terminal
```

### Error: "No se puede conectar al backend"

1. Verificar que el backend está corriendo: `http://<TU_IP>:5000/health`
2. Comprobar firewall de Windows
3. Verificar que dispositivo está en la misma red WiFi
4. Revisar IP configurada en los servicios de la app

### Error al compilar: "workload not found"

```powershell
# Reinstalar MAUI workload
dotnet workload install maui --skip-manifest-update
```

### Dispositivo no detectado (adb devices vacío)

1. Instalar drivers USB del fabricante del dispositivo
2. Verificar cable USB (usar cable de datos, no solo carga)
3. Cambiar modo USB en el dispositivo: **File Transfer** o **PTP**
4. Reiniciar adb:
   ```powershell
   adb kill-server
   adb start-server
   adb devices
   ```

### Aplicación compilada pero crash al iniciar

```powershell
# Ver logs en tiempo real
adb logcat | Select-String "Mobile.Credential|Mobile.AccessPoint|AndroidRuntime"

# Filtrar solo errores
adb logcat *:E
```

---

## 📚 Documentación Específica

- [Mobile.Credential README](src/Mobile.Credential/README.md) - Detalles de la app de credencial
- [Mobile.AccessPoint README](src/Mobile.AccessPoint/README.md) - Detalles de la app de punto de control

---

## 👥 Soporte

Si encuentras problemas no listados aquí, contacta al equipo:
- Nadia Gorría
- Joaquín Jozami
- Salvador Vanoli
- Valentín Veintemilla

O abre un issue en el repositorio de GitHub.

---

## ✅ Checklist de Setup Completo

- [ ] .NET 8 SDK instalado y verificado
- [ ] MAUI workload instalado
- [ ] JDK 17 instalado y JAVA_HOME configurado
- [ ] Android SDK instalado y ANDROID_HOME configurado
- [ ] adb funcional (dispositivos detectados)
- [ ] Dispositivo Android en modo desarrollador
- [ ] Depuración USB habilitada
- [ ] NFC habilitado en el dispositivo
- [ ] Repositorio clonado
- [ ] Mobile.Credential compila exitosamente
- [ ] Mobile.AccessPoint compila exitosamente
- [ ] Backend corriendo localmente
- [ ] IP del backend actualizada en las apps
- [ ] Aplicaciones instaladas en dispositivos
- [ ] Login exitoso en ambas apps
- [ ] NFC funcionando entre dispositivos

---

**¡Listo para desarrollar! 🚀**
