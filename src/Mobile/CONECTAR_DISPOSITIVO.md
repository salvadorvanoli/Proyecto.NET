# Conectar Dispositivo Android

## 1. Habilitar Modo Desarrollador

1. Ve a **Configuración** → **Acerca del teléfono**
2. Busca **Número de compilación** (puede estar en "Información del software")
3. Toca **7 veces seguidas** en "Número de compilación"
4. Aparecerá un mensaje: "Ahora eres desarrollador"

## 2. Habilitar Depuración USB

1. Regresa a **Configuración**
2. Verás una nueva opción: **Opciones de desarrollador** o **Developer options**
3. Entra y activa:
   - ✅ **Depuración USB** (USB debugging)
   - ✅ **Instalar vía USB** (si está disponible)
   - ✅ **Permanecer activo** (opcional, evita que se apague la pantalla)

## 3. Conectar via USB

1. Conecta el cable USB de tu celular a la PC
2. En el celular aparecerá un mensaje:
   - "¿Permitir depuración USB?"
   - "Huella digital RSA: ..."
3. ✅ Marca "Permitir siempre desde esta computadora"
4. Toca **Aceptar** o **Permitir**

## 4. Verificar Conexión

Ejecuta en PowerShell:
```powershell
cd c:\Nadia\.NET\Proyecto.NET\src\Mobile
dotnet build -t:AdbDevices
```

Deberías ver algo como:
```
List of devices attached
ABC123456789    device
```

Si aparece tu dispositivo, ¡está conectado correctamente!

## Troubleshooting

### El dispositivo no aparece:
```powershell
# Reiniciar ADB
adb kill-server
adb start-server
adb devices
```

### Aparece como "unauthorized":
- Desconecta y reconecta el USB
- Revoca permisos: Opciones de desarrollador → Revocar autorizaciones de depuración USB
- Vuelve a conectar y acepta el mensaje

### No aparece el mensaje de depuración:
- Cambia el modo USB: Desliza la barra de notificaciones → Toca "USB" → Selecciona "Transferencia de archivos" o "PTP"
- Desactiva y reactiva "Depuración USB"

## Una vez conectado:

Ejecuta:
```powershell
cd c:\Nadia\.NET\Proyecto.NET\src\Mobile
dotnet build -t:Run -f net8.0-android
```

¡La app se instalará y ejecutará en tu celular!
