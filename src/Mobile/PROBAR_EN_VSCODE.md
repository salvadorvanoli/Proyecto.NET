# 🎯 GUÍA RÁPIDA: Probar MAUI desde VS Code

## ⚡ Opción MÁS FÁCIL: Ver la App sin ejecutarla

Como MAUI en Windows tiene problemas con WindowsAppSDK en tu sistema, la forma más práctica es:

### 📸 Ver Screenshots/Demo:
La app ya está funcional, solo necesitas verla. Opciones:

1. **Usar Android Emulator** (recomendado)
2. **Usar dispositivo Android físico** (si tienes uno a mano)
3. **Usar Visual Studio 2022** (tiene mejor soporte que VS Code para MAUI)

---

## 🤖 OPCIÓN RECOMENDADA: Android Emulator

### Paso 1: Instalar Android Studio (una vez)

1. Descarga Android Studio: https://developer.android.com/studio
2. Durante instalación, selecciona:
   - ✅ Android SDK
   - ✅ Android Virtual Device (AVD)
3. Instala (toma ~10 minutos)

### Paso 2: Crear un Emulador (una vez)

1. Abre Android Studio
2. Clic en "More Actions" → "Virtual Device Manager"
3. Clic en "+ Create Device"
4. Selecciona "Phone" → "Pixel 5" → Next
5. Descarga "Tiramisu" (API 33) o "UpsideDownCake" (API 34)
6. Next → Finish

### Paso 3: Ejecutar la App MAUI

Desde VS Code:

**Método 1 - Usando Tasks (más fácil):**
1. Presiona `Ctrl + Shift + P`
2. Escribe: `Tasks: Run Task`
3. Selecciona: `🤖 Run MAUI App (Android)`

**Método 2 - Terminal:**
```powershell
cd "c:\Nadia\.NET\Proyecto.NET\src\Mobile"
dotnet build Mobile.csproj -t:Run -f net8.0-android
```

Esto automáticamente:
- ✅ Compila la app
- ✅ Inicia el emulador
- ✅ Instala la app
- ✅ La ejecuta

---

## 📱 ALTERNATIVA: Dispositivo Físico Android

### Si tienes un smartphone Android:

1. **Habilita USB Debugging:**
   ```
   Configuración → Acerca del teléfono → 
   Toca "Número de compilación" 7 veces →
   Volver → Opciones de desarrollador → 
   Activa "USB Debugging"
   ```

2. **Conecta por USB**

3. **Verifica conexión:**
   ```powershell
   # Si da error, instala platform-tools:
   # https://developer.android.com/tools/releases/platform-tools
   
   adb devices
   ```

4. **Ejecuta:**
   ```powershell
   cd "c:\Nadia\.NET\Proyecto.NET\src\Mobile"
   dotnet build Mobile.csproj -t:Run -f net8.0-android
   ```

---

## 🖥️ ALTERNATIVA: Usar Visual Studio 2022

VS Code no tiene buen soporte para MAUI Windows. Si quieres ejecutar en Windows:

1. Descarga Visual Studio 2022 Community (gratis): https://visualstudio.microsoft.com/
2. Durante instalación, selecciona: ".NET Multi-platform App UI development"
3. Abre: `c:\Nadia\.NET\Proyecto.NET\Proyecto.NET.sln`
4. Clic derecho en `Mobile` → "Set as Startup Project"
5. Selecciona "Windows Machine" en la barra
6. Presiona F5

---

## 🎮 Qué verás cuando ejecutes la app

### Página 1: Home (MainPage)
- Título de bienvenida
- Botón "Click me"
- Contador

### Página 2: Validación NFC (AccessNfcPage)
Para ir ahí:
1. Toca el ícono ☰ (esquina superior izquierda)
2. Selecciona "Validación NFC"

Verás:
- ✅ Estado de NFC (Disponible/Habilitado)
- 🔘 Botón "Iniciar Escaneo"
- Después de 3 segundos: **Simulación de tag NFC**
- Resultado: Acceso Permitido/Denegado
- Detalles del punto de control

### Funcionalidad NFC:
- ⚠️ **En emulador**: Simulación (no NFC real)
- ✅ **En dispositivo físico**: Simulación (hasta que implementes NFC nativo con `EJEMPLO_NFC_NATIVO_ANDROID.cs`)

---

## 🐛 Solución de Problemas

### Error: "No se encuentra el emulador"
```powershell
# Agregar Android SDK al PATH
# Ruta típica:
$env:PATH += ";C:\Users\[TuUsuario]\AppData\Local\Android\Sdk\platform-tools"
$env:PATH += ";C:\Users\[TuUsuario]\AppData\Local\Android\Sdk\emulator"

# Verificar
emulator -list-avds
```

### Error: "Class not registered" (Windows)
Este es el error que tienes. Soluciones:
1. Usar Android en vez de Windows
2. Instalar Visual Studio 2022 (tiene las DLLs necesarias)
3. Instalar Windows App SDK: https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads

### Error: "BUILD FAILED"
```powershell
cd "c:\Nadia\.NET\Proyecto.NET\src\Mobile"
dotnet clean
dotnet restore
dotnet build
```

### La app no responde en el emulador
- Espera ~30 segundos (el emulador es lento la primera vez)
- Verifica que el emulador esté completamente iniciado (ves el home de Android)
- Mira los logs: `adb logcat | Select-String "Mobile"`

---

## ✅ Checklist de Testing

### Para verificar que todo funciona:

- [ ] La app se ejecuta sin crashes
- [ ] Puedes abrir el menú lateral (☰)
- [ ] Puedes navegar a "Validación NFC"
- [ ] El botón "Iniciar Escaneo" funciona
- [ ] Después de 3 seg, se detecta un tag simulado
- [ ] Se muestra el resultado (Acceso Permitido/Denegado)
- [ ] Se ve el nombre del punto de control
- [ ] La app se detiene automáticamente después de 5 seg

---

## 🎯 Resumen: ¿Qué opción elegir?

| Tu situación | Opción recomendada |
|--------------|-------------------|
| Solo quiero ver si funciona rápido | 📱 **Dispositivo físico** (si tienes) |
| Quiero testear bien sin hardware | 🤖 **Android Emulator** |
| Necesito debugging avanzado | 🖥️ **Visual Studio 2022** |
| Quiero probar NFC real | 📱 **Dispositivo físico + tags NFC** |

---

## 🚀 Mi Recomendación para TI

**Paso 1 (Ahora - 5 minutos):**
```powershell
cd "c:\Nadia\.NET\Proyecto.NET\src\Mobile"
dotnet build Mobile.csproj -t:Run -f net8.0-android
```

Si no tienes emulador, te dirá que instales Android Studio.

**Paso 2 (Si da error - 20 minutos):**
1. Instala Android Studio
2. Crea un emulador Pixel 5 con API 33
3. Vuelve a ejecutar el comando

**Paso 3 (Futuro):**
Cuando necesites NFC real, usa `EJEMPLO_NFC_NATIVO_ANDROID.cs` como guía.

---

**¿Algún error?** Copia el mensaje de error completo y te ayudo a solucionarlo.
