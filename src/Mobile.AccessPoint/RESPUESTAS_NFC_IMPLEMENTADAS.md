# ✅ Respuestas Visuales NFC - Implementación Completa

## 📋 Resumen

Se ha implementado la funcionalidad completa para que el **punto de control** (Mobile.AccessPoint) envíe respuestas visuales al **celular con credencial** (Mobile) mediante NFC.

## 🎯 Funcionalidad Implementada

### Cuando un usuario pasa su credencial por el punto de control:

1. **Punto de control lee credencial** → `NfcServiceAndroid.ProcessNfcTag()`
2. **Valida con backend** → `AccessNfcViewModel.ValidateAccessAsync()`
3. **Envía respuesta visual via NFC**:
   - ✅ `SendAccessGrantedAsync("✅ Acceso concedido")` si tiene permiso
   - ❌ `SendAccessDeniedAsync("❌ Acceso denegado")` si no tiene permiso
4. **Celular con credencial recibe alerta** → Muestra popup en pantalla

## 📁 Archivos Modificados

### Mobile.AccessPoint (Punto de Control)

#### 1. `Services/INfcService.cs`
```csharp
// Agregados métodos de respuesta
Task<bool> SendAccessGrantedAsync(string message = "Acceso concedido");
Task<bool> SendAccessDeniedAsync(string message = "Acceso denegado");
```

#### 2. `Services/NfcService.cs`
```csharp
// Implementación multiplataforma
public virtual async Task<bool> SendAccessGrantedAsync(string message)
{
#if ANDROID
    return await SendAccessGrantedAndroidAsync(message);
#else
    return false;
#endif
}
```

#### 3. `Platforms/Android/NfcServiceAndroid.cs`
**CAMBIOS CLAVE:**
- Agregado campo `_currentIsoDep` para mantener conexión abierta
- Modificado `ProcessNfcTag()` para NO cerrar conexión después de leer
- Implementados métodos:
  - `SendAccessGrantedAndroidAsync()` - Envía comando `00 AC 01 00` + mensaje
  - `SendAccessDeniedAndroidAsync()` - Envía comando `00 AC 00 00` + mensaje
  - `CloseCurrentIsoDep()` - Cierra conexión después de enviar respuesta

```csharp
private IsoDep? _currentIsoDep;

private void ProcessNfcTag(Tag tag)
{
    var isoDep = IsoDep.Get(tag);
    isoDep.Connect();
    _currentIsoDep = isoDep; // ✅ Mantener abierto
    
    // Leer credencial...
    // NO llamar isoDep.Close() aquí ❌
}

private async Task<bool> SendAccessGrantedAndroidAsync(string message)
{
    byte[] command = new byte[4 + messageBytes.Length];
    command[0] = 0x00;  // CLA
    command[1] = 0xAC;  // INS - Access Control
    command[2] = 0x01;  // P1 - Granted (1 = concedido)
    command[3] = 0x00;  // P2
    
    var response = _currentIsoDep.Transceive(command);
    CloseCurrentIsoDep(); // ✅ Cerrar después de respuesta
    
    return response[^2] == 0x90 && response[^1] == 0x00;
}
```

#### 4. `ViewModels/AccessNfcViewModel.cs`
**Integración en flujo de validación:**

```csharp
private async void OnNfcTagDetected(object? sender, NfcTagDetectedEventArgs e)
{
    // 1. Validar acceso con backend
    var validationResult = await ValidateAccessAsync(userId, controlPointId);
    
    // 2. Crear evento en backend
    await _accessEventApiService.CreateAccessEventAsync(request);
    
    // 3. 🆕 ENVIAR RESPUESTA VISUAL
    if (isDigitalCredential)
    {
        if (validationResult.IsGranted)
        {
            await _nfcService.SendAccessGrantedAsync("✅ Acceso concedido");
        }
        else
        {
            await _nfcService.SendAccessDeniedAsync($"❌ {validationResult.Reason}");
        }
    }
    
    // 4. Mostrar resultado en pantalla del punto de control
    ShowAccessResult(validationResult, tagId, eventDateTime);
}
```

### Mobile (Credencial) - Ya Implementado Anteriormente

Los siguientes archivos ya fueron modificados en la implementación anterior:

- `Services/INfcCredentialService.cs` - Evento `AccessResponseReceived`
- `Platforms/Android/Services/NfcHostCardEmulationService.cs` - Procesamiento de comandos
- `ViewModels/CredentialViewModel.cs` - Alertas visuales

## 🔄 Flujo Completo

```
┌────────────────────┐                  ┌─────────────────────┐
│  Mobile            │                  │ Mobile.AccessPoint  │
│  (Credencial)      │                  │ (Punto de Control)  │
└────────────────────┘                  └─────────────────────┘
         │                                        │
         │  1. Usuario activa credencial         │
         │────────────────────────────────────>  │
         │                                        │
         │  2. Acerca celular al lector          │
         │  <────────────────────────────────────│
         │                                        │
         │  3. SELECT AID                         │
         │  <────────────────────────────────────│
         │  Response: 90 00                       │
         │────────────────────────────────────>  │
         │                                        │
         │  4. GET DATA                           │
         │  <────────────────────────────────────│
         │  Response: CRED:123|USER:456           │
         │────────────────────────────────────>  │
         │                                        │
         │           [Valida con backend]         │
         │                                        │
         │  5a. ACCESS GRANTED (00 AC 01 00)      │
         │      + "✅ Acceso concedido"            │
         │  <────────────────────────────────────│
         │  Response: 90 00                       │
         │────────────────────────────────────>  │
         │                                        │
         │  ✅ Muestra alerta "Acceso Concedido" │
         │                                        │
```

## 🧪 Cómo Probar

### Requisitos:
- 2 celulares Android con NFC
- Backend corriendo (para validación)

### Pasos:

1. **Celular A (Credencial)**
   - Ejecutar proyecto `Mobile`
   - Iniciar sesión
   - Ir a página de Credencial
   - Presionar "🚀 Activar Credencial"

2. **Celular B (Punto de Control)**
   - Ejecutar proyecto `Mobile.AccessPoint`
   - Iniciar sesión
   - Ir a página NFC
   - Presionar "Iniciar Lectura"
   - Configurar ID del punto de control

3. **Realizar lectura**
   - Acercar celular A (con credencial activa) a celular B
   - Back-to-back, mantener 1-2 segundos

4. **Observar resultados**
   - **Celular B**: Muestra resultado en pantalla (verde/rojo)
   - **Celular A**: Recibe alerta popup con resultado
   - **Backend**: Registra evento de acceso

### Logs Esperados

**Mobile (Credencial):**
```
📩 Access response received: True - Acceso concedido
```

**Mobile.AccessPoint (Punto de Control):**
```
📤 Sending ACCESS GRANTED to credential device: ✅ Acceso concedido
✅ ACCESS GRANTED sent successfully
✅ Visual response sent to credential device successfully
```

## 🔧 Personalización

### Cambiar mensajes de respuesta

En `AccessNfcViewModel.cs`, línea ~XXX:

```csharp
// Personalizar mensaje de acceso concedido
await _nfcService.SendAccessGrantedAsync("¡Bienvenido! 🎉");

// Personalizar mensaje con información adicional
await _nfcService.SendAccessDeniedAsync($"Sin permisos - {validationResult.Reason}");
```

### Agregar información al mensaje

```csharp
string message = $"✅ Acceso: {validationResult.ControlPointName}\n{DateTime.Now:HH:mm}";
await _nfcService.SendAccessGrantedAsync(message);
```

## ⚠️ Consideraciones

### Timing
- La comunicación debe completarse en < 2 segundos
- Si el usuario aleja el celular antes, no recibirá respuesta
- El log mostrará "Could not send visual response (device may have moved away)"

### Manejo de Errores
- Si falla el envío de respuesta, NO afecta la validación
- El evento ya fue registrado en backend
- Solo afecta el feedback visual al usuario

### Conexión NFC
- La conexión ISO-DEP se mantiene abierta entre lectura y respuesta
- Se cierra automáticamente después de enviar respuesta
- Timeout típico: 1-2 segundos

## 📚 Referencias

Ver `src/Mobile/PROTOCOLO_NFC_BIDIRECCIONAL.md` para detalles técnicos del protocolo APDU.
