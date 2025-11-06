# 🚀 Cómo Ejecutar la App MAUI desde VS Code

## ✅ Opción 1: Usar Tasks (Recomendado)

Ya tienes configuradas las tareas en `.vscode/tasks.json`:

### 📝 Pasos:
1. Presiona `Ctrl + Shift + P` (abre la paleta de comandos)
2. Escribe: `Tasks: Run Task`
3. Selecciona una de estas opciones:
   - **🖥️ Run MAUI App (Windows)** - Ejecuta en Windows
   - **🤖 Run MAUI App (Android Emulator)** - Ejecuta en emulador Android
   - **🔨 Build MAUI App (All Platforms)** - Solo compila

### ⌨️ Atajo rápido:
- `Ctrl + Shift + B` → Ejecuta el build por defecto

---

## 🖥️ Opción 2: Terminal Integrado (Rápido)

### Para Windows:
```powershell
cd src/Mobile
dotnet build -t:Run -f net8.0-windows10.0.19041.0 -p:WindowsPackageType=None
```

### Para Android (requiere emulador):
```powershell
cd src/Mobile
dotnet build -t:Run -f net8.0-android
```

---

## 🤖 Opción 3: Android Emulator (Requiere configuración)

### Pre-requisitos:
1. **Android SDK** instalado
2. **Emulador Android** configurado

### Verificar instalación:
```powershell
# Ver workloads instalados
dotnet workload list

# Si no está Android, instalar:
dotnet workload install android
```

### Listar emuladores disponibles:
```powershell
cd src/Mobile
dotnet build -t:Run -f net8.0-android
```

El comando automáticamente:
- Inicia el emulador si está apagado
- Instala la app
- La ejecuta

### Crear un emulador nuevo:
1. Instala Android Studio
2. Abre AVD Manager (Android Virtual Device)
3. Crea un dispositivo (ej: Pixel 5, API 34)
4. O usa línea de comandos:
   ```powershell
   # Listar emuladores
   emulator -list-avds
   
   # Iniciar emulador específico
   emulator -avd Pixel_5_API_34
   ```

---

## 📱 Opción 4: Dispositivo Físico Android (Para NFC Real)

### 1. Habilitar USB Debugging:
```
Configuración → Acerca del teléfono → 
Tocar "Número de compilación" 7 veces →
Volver → Opciones de desarrollador → 
Activar "USB Debugging"
```

### 2. Conectar por USB:
```powershell
# Verificar que el dispositivo esté conectado
adb devices
```

Deberías ver algo como:
```
List of devices attached
ABC123456789    device
```

### 3. Ejecutar en el dispositivo:
```powershell
cd src/Mobile
dotnet build -t:Run -f net8.0-android
```

La app se instalará automáticamente en el dispositivo conectado.

---

## 🎯 Recomendaciones según tu objetivo:

### 🧪 **Para Testing General (UI/UX)**
→ Usa **Windows** (Opción 1 o 2)
- ✅ Más rápido
- ✅ No requiere configuración
- ✅ Ideal para desarrollo de interfaces
- ⚠️ No tiene NFC real

### 🤖 **Para Testing Android (Sin NFC)**
→ Usa **Emulador Android** (Opción 3)
- ✅ Simula Android real
- ✅ Puedes probar gestos, navegación
- ⚠️ Más lento que Windows
- ⚠️ No tiene NFC real

### 📱 **Para Testing NFC Real**
→ Usa **Dispositivo Físico** (Opción 4)
- ✅ NFC real funciona
- ✅ Performance real
- ✅ Testing completo
- ⚠️ Requiere hardware (smartphone + tags NFC)

---

## 🐛 Troubleshooting

### Error: "No se encuentra el emulador"
```powershell
# Instalar Android workload
dotnet workload install android

# Verificar
dotnet workload list
```

### Error: "WindowsAppSDK not found"
```powershell
# Limpiar y reconstruir
cd src/Mobile
dotnet clean
dotnet build
```

### La app no se ve bien en Windows
- La app MAUI está optimizada para móviles
- La ventana puede verse pequeña en Windows
- Esto es normal, la UI está pensada para pantallas móviles

### Error: "adb not found"
Necesitas agregar Android SDK al PATH:
```powershell
# Buscar la ruta (usualmente):
C:\Program Files (x86)\Android\android-sdk\platform-tools

# O si instalaste via Android Studio:
C:\Users\[TuUsuario]\AppData\Local\Android\Sdk\platform-tools
```

---

## 📊 Comparación de Opciones

| Opción | Velocidad | NFC Real | Facilidad | Recomendado para |
|--------|-----------|----------|-----------|------------------|
| Windows | ⚡⚡⚡ Muy rápido | ❌ No | ✅ Fácil | Desarrollo UI |
| Emulador Android | ⚡ Lento | ❌ No | ⚠️ Media | Testing Android |
| Dispositivo Android | ⚡⚡ Medio | ✅ Sí | ⚠️ Media | Testing NFC |
| iOS Simulator | ⚡⚡ Medio | ❌ No | ❌ Difícil* | Testing iOS |

*iOS requiere Mac para compilar

---

## ✨ Próximos Pasos

1. **Ahora**: Ejecuta en Windows para ver la app funcionando
2. **Luego**: Conecta al backend API (http://localhost:5000)
3. **Después**: Implementa autenticación
4. **Finalmente**: Prueba NFC real con dispositivo físico

---

## 🆘 Comandos Útiles

```powershell
# Ver información del proyecto
cd src/Mobile
dotnet build -v:n

# Limpiar completamente
dotnet clean
rm -r bin,obj -Force

# Restaurar paquetes
dotnet restore

# Ver dispositivos Android conectados
adb devices

# Ver logs de Android en tiempo real
adb logcat | Select-String "Mobile"

# Desinstalar app del dispositivo
adb uninstall com.companyname.mobile
```

---

**¿Necesitas ayuda?** 
- Si hay errores, ejecuta primero: `dotnet clean` y luego `dotnet build`
- Revisa el terminal para ver mensajes de error específicos
- Consulta `GUIA_USO_MAUI.md` para más detalles
