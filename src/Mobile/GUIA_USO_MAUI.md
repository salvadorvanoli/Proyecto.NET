# 📱 Guía de Uso MAUI - Control de Accesos NFC

## 🚀 Cómo ejecutar la aplicación MAUI

### Opción 1: Desde Visual Studio 2022
1. Abre `Proyecto.NET.sln` en Visual Studio 2022
2. Selecciona el proyecto `Mobile` como proyecto de inicio
3. En la barra de herramientas, selecciona la plataforma:
   - **Android Emulator** (Google Pixel, etc.)
   - **Windows Machine** (para probar en Windows)
   - **iOS Simulator** (requiere Mac conectado)
4. Presiona F5 o haz clic en "Start Debugging"

### Opción 2: Desde línea de comandos

#### Para Android:
```powershell
cd c:\Nadia\.NET\Proyecto.NET\src\Mobile

# Listar dispositivos/emuladores disponibles
dotnet build -t:Run -f net8.0-android

# O especificar un emulador
dotnet build -t:Run -f net8.0-android -p:AndroidEmulator=pixel_5
```

#### Para Windows:
```powershell
cd c:\Nadia\.NET\Proyecto.NET\src\Mobile
dotnet build -t:Run -f net8.0-windows10.0.19041.0 -p:WindowsPackageType=None
```

## 📋 Navegación en la App

La aplicación tiene un menú lateral (Flyout) con las siguientes opciones:

1. **Inicio**: Página principal de bienvenida
2. **Validación NFC**: Página para escanear tags NFC

Para abrir el menú:
- **Android/iOS**: Desliza desde el borde izquierdo o toca el ícono ☰
- **Windows**: Clic en el ícono ☰

## 🧪 Cómo Probar NFC

### 🔹 Modo Simulación (Actual)
La implementación actual **simula** la detección NFC para que puedas probar sin hardware:

1. Ejecuta la app en cualquier emulador/dispositivo
2. Navega a "Validación NFC"
3. Presiona "Iniciar Escaneo"
4. **Automáticamente después de 3 segundos**, se simulará la detección de un tag NFC
5. Verás el resultado (acceso permitido/denegado) con animaciones

**Ventajas del modo simulación:**
- ✅ No requiere hardware NFC
- ✅ Funciona en emuladores Windows/Android
- ✅ Útil para desarrollo de UI/UX
- ✅ Simula puntos de control aleatorios (ID 1-6)

### 🔹 NFC Real - ¿Qué Necesitas?

Para usar **NFC real**, necesitas:

#### 1. Hardware NFC
- **Dispositivo Android físico** con chip NFC (la mayoría de smartphones desde 2015+)
  - Ejemplos: Samsung Galaxy, Google Pixel, OnePlus, Xiaomi
- **iPhone** con iOS 13+ (iPhone 7 o superior)
- **Tags NFC** para programar (tipo NTAG213, NTAG215, NTAG216)

#### 2. Verificar que tu dispositivo tiene NFC
**Android:**
```
Configuración > Conexiones > NFC y pago
```
Debe estar activado el switch de NFC.

**iPhone:**
El NFC siempre está activo, solo necesitas una app con permisos CoreNFC.

#### 3. Conseguir Tags NFC
Puedes comprar tags NFC en:
- Amazon (paquete de 10-20 tags ~$10-15 USD)
- AliExpress (más baratos, toma más tiempo)
- Tiendas de electrónica locales

**Tipos recomendados:**
- NTAG213: 144 bytes, perfecto para IDs simples
- NTAG215: 504 bytes, compatible con Amiibo
- NTAG216: 888 bytes, más espacio para datos

## 🔧 Implementar NFC Real en Android

### Paso 1: Implementación Nativa Android

El código actual tiene la estructura lista. Para NFC real, implementa:

```csharp
// En Mobile/Platforms/Android/NfcServiceAndroid.cs
#if ANDROID
using Android.Nfc;
using Android.App;
using Android.Content;

namespace Mobile.Services;

public partial class NfcService
{
    private NfcAdapter? _nfcAdapter;
    private Activity? _activity;

    public override bool IsAvailable
    {
        get
        {
            _nfcAdapter ??= NfcAdapter.GetDefaultAdapter(Platform.CurrentActivity);
            return _nfcAdapter != null;
        }
    }

    public override bool IsEnabled
    {
        get
        {
            _nfcAdapter ??= NfcAdapter.GetDefaultAdapter(Platform.CurrentActivity);
            return _nfcAdapter?.IsEnabled ?? false;
        }
    }

    public override Task StartListeningAsync()
    {
        _activity = Platform.CurrentActivity;
        _nfcAdapter = NfcAdapter.GetDefaultAdapter(_activity);

        if (_nfcAdapter == null)
            throw new NotSupportedException("NFC not available");

        if (!_nfcAdapter.IsEnabled)
            throw new InvalidOperationException("NFC not enabled");

        var intent = new Intent(_activity, _activity.GetType())
            .AddFlags(ActivityFlags.SingleTop);
        var pendingIntent = PendingIntent.GetActivity(
            _activity, 0, intent, PendingIntentFlags.Mutable);

        var filters = new IntentFilter[] { new IntentFilter(NfcAdapter.ActionNdefDiscovered) };
        
        _nfcAdapter.EnableForegroundDispatch(_activity, pendingIntent, filters, null);
        
        _isListening = true;
        return Task.CompletedTask;
    }
}
#endif
```

### Paso 2: Programar Tags NFC

Para programar los tags con información de puntos de control:

1. **Descarga una app de programación NFC**:
   - Android: "NFC Tools" (gratuita)
   - iPhone: "NFC Tools" o "NFC TagWriter"

2. **Formato del mensaje NDEF**:
   - Tipo: Text Record
   - Contenido: `CONTROL_POINT:{id}:{nombre}`
   
   Ejemplos:
   ```
   CONTROL_POINT:1:Entrada Principal
   CONTROL_POINT:2:Salida Principal
   CONTROL_POINT:3:Entrada Estacionamiento
   ```

3. **Programar el tag**:
   - Abre NFC Tools
   - Ve a "Write"
   - Selecciona "Add a record" > "Text"
   - Escribe el mensaje (ej: `CONTROL_POINT:1:Entrada Principal`)
   - Toca "Write" y acerca el tag a tu teléfono

### Paso 3: Probar con Tags Reales

1. Conecta tu dispositivo Android físico vía USB
2. Habilita "USB Debugging" en el dispositivo:
   ```
   Configuración > Acerca del teléfono > 
   Toca "Número de compilación" 7 veces >
   Vuelve > Opciones de desarrollador > USB Debugging
   ```
3. Ejecuta desde Visual Studio seleccionando tu dispositivo
4. Acerca un tag NFC programado al dispositivo
5. La app detectará el tag y mostrará el punto de control

## 🔍 Probar NFC Real SIN Hardware (alternativas)

Si no tienes tags NFC físicos pero quieres probar:

### 1. Emular tag con otro dispositivo Android
Usa la app "NFC Card Emulator" (requiere root en algunos dispositivos)

### 2. Usar tarjetas NFC comunes
Muchas tarjetas que ya tienes pueden funcionar:
- Tarjetas de transporte público (ej: BIP en Chile)
- Tarjetas de acceso de edificios
- Tarjetas de fidelidad de tiendas

**Nota:** Estas tarjetas pueden ser de solo lectura o tener datos encriptados.

### 3. Probar con stickers NFC baratos
Los stickers NFC más baratos (NTAG203) funcionan y cuestan ~$0.50 USD cada uno.

## 📊 Comparación: Simulación vs NFC Real

| Característica | Simulación (Actual) | NFC Real |
|----------------|---------------------|----------|
| Funciona en emulador | ✅ Sí | ❌ No |
| Requiere hardware | ❌ No | ✅ Sí (dispositivo + tags) |
| Desarrollo UI/UX | ✅ Perfecto | ⚠️ Requiere deploy físico |
| Testing rápido | ✅ Instantáneo | ⚠️ Más lento |
| Validación real | ❌ No | ✅ Sí |
| Costo | 💰 Gratis | 💰 ~$20 USD (tags + tiempo) |
| Tiempo de setup | ⏱️ 0 min | ⏱️ 1-2 horas |

## 🎯 Recomendación

**Para desarrollo actual:**
1. ✅ Usa la **simulación** para desarrollar toda la UI/UX
2. ✅ Conecta la app al backend (crear eventos de acceso)
3. ✅ Implementa autenticación y navegación
4. ✅ Prueba el flujo completo con datos simulados

**Cuando esté listo para producción:**
1. 📱 Consigue 1-2 tags NFC para testing (~$2 USD)
2. 🔧 Implementa la clase `NfcServiceAndroid.cs` con APIs nativas
3. 🧪 Prueba en dispositivo físico
4. 📋 Documenta el proceso de programación de tags para el cliente

## 🐛 Troubleshooting

### "No se puede ejecutar en emulador Android"
**Solución:** Asegúrate de tener el Android SDK instalado:
```powershell
# Verificar instalación
dotnet workload list

# Instalar si falta
dotnet workload install android
```

### "NFC not working on physical device"
1. Verifica que NFC esté habilitado en Configuración
2. Algunos dispositivos requieren que la pantalla esté encendida
3. Acerca el tag al área correcta (generalmente parte superior trasera)

### "Build failed for iOS"
iOS requiere un Mac para compilar. Opciones:
- Conecta Visual Studio a un Mac remoto
- Usa Mac Build Host en red local
- Prueba solo en Android/Windows por ahora

## 📚 Recursos Adicionales

- [Documentación oficial MAUI](https://learn.microsoft.com/dotnet/maui/)
- [Android NFC Guide](https://developer.android.com/guide/topics/connectivity/nfc)
- [iOS CoreNFC](https://developer.apple.com/documentation/corenfc)
- [NFC Tools App](https://www.wakdev.com/en/apps/nfc-tools-pc-mac.html)

## ✅ Checklist de Testing

### Modo Simulación (Ahora)
- [ ] La app compila sin errores
- [ ] Puedo navegar a "Validación NFC"
- [ ] El botón "Iniciar Escaneo" funciona
- [ ] Se muestra la simulación después de 3 segundos
- [ ] El resultado se muestra con colores correctos
- [ ] La app se detiene automáticamente después de 5 segundos

### NFC Real (Futuro)
- [ ] Tengo un dispositivo Android con NFC
- [ ] Tengo al menos 1 tag NFC
- [ ] He programado el tag con el formato correcto
- [ ] La app detecta el tag real
- [ ] El evento se registra en el backend
- [ ] El historial muestra el evento creado

---

**¿Necesitas ayuda?** Consulta este documento o pregunta específicamente sobre:
- Cómo ejecutar en un emulador específico
- Cómo implementar NFC nativo para Android/iOS
- Cómo programar tags NFC
- Cómo conectar al backend desde MAUI
