# Protocolo NFC Bidireccional - Respuestas Visuales del Punto de Control

## ✅ IMPLEMENTACIÓN COMPLETA

Este documento describe el protocolo NFC bidireccional **YA IMPLEMENTADO** para que el **celular con la credencial** (Mobile) reciba respuestas visuales del **punto de control** (Mobile.AccessPoint) cuando pasa por un lector NFC.

## 📋 Descripción General

```
┌─────────────────────┐                    ┌──────────────────────┐
│  Mobile             │                    │ Mobile.AccessPoint   │
│  (Credencial)       │                    │ (Punto de Control)   │
└─────────────────────┘                    └──────────────────────┘
         │                                            │
         │  1. Activa HCE (Host Card Emulation)      │
         │─────────────────────────────────────────> │
         │                                            │
         │  2. Acerca celular al lector NFC          │
         │ <─────────────────────────────────────────│
         │                                            │
         │  3. SELECT AID (00 A4 04 00)              │
         │ <─────────────────────────────────────────│
         │  Response: 90 00 (OK)                     │
         │─────────────────────────────────────────> │
         │                                            │
         │  4. GET DATA (00 CA 00 00 00)             │
         │ <─────────────────────────────────────────│
         │  Response: CRED:123|USER:456 + 90 00      │
         │─────────────────────────────────────────> │
         │                                            │
         │         [Control Point valida credencial]  │
         │                                            │
         │  5a. ACCESS GRANTED (00 AC 01 00)         │
         │      + "Acceso concedido" (opcional)      │
         │ <─────────────────────────────────────────│
         │  Response: 90 00                          │
         │─────────────────────────────────────────> │
         │                                            │
         │  ✅ Muestra alerta "Acceso Concedido"    │
         │                                            │
         │         --- O ---                          │
         │                                            │
         │  5b. ACCESS DENIED (00 AC 00 00)          │
         │      + "Acceso denegado" (opcional)       │
         │ <─────────────────────────────────────────│
         │  Response: 90 00                          │
         │─────────────────────────────────────────> │
         │                                            │
         │  ❌ Muestra alerta "Acceso Denegado"     │
         │                                            │
```

## 📡 Comandos APDU Definidos

### 1. SELECT AID
- **Comando**: `00 A4 04 00` + longitud AID + AID
- **AID**: `F0 39 41 48 14 81 00`
- **Propósito**: Seleccionar la aplicación de credencial digital
- **Respuesta esperada**: `90 00` (OK)

### 2. GET DATA
- **Comando**: `00 CA 00 00 00`
- **Propósito**: Obtener datos de la credencial
- **Respuesta**: `CRED:{credentialId}|USER:{userId}` + `90 00`
- **Ejemplo**: `CRED:123|USER:456` + `90 00`

### 3. ACCESS GRANTED (NUEVO)
- **Comando**: `00 AC 01 00` + (opcional) mensaje UTF-8
- **Propósito**: Notificar al celular que el acceso fue concedido
- **Respuesta esperada**: `90 00`
- **Mensaje opcional**: Puede incluir texto personalizado como "Bienvenido", "Acceso autorizado", etc.

### 4. ACCESS DENIED (NUEVO)
- **Comando**: `00 AC 00 00` + (opcional) mensaje UTF-8
- **Propósito**: Notificar al celular que el acceso fue denegado
- **Respuesta esperada**: `90 00`
- **Mensaje opcional**: Puede incluir motivo como "Sin permisos", "Horario no válido", etc.

## 💻 Implementación en el Punto de Control (Mobile.AccessPoint)

### ✅ IMPLEMENTACIÓN COMPLETA

La implementación ya está lista en los siguientes archivos:

1. **`INfcService.cs`**: Interfaz con métodos `SendAccessGrantedAsync()` y `SendAccessDeniedAsync()`
2. **`NfcService.cs`**: Implementación base con soporte multiplataforma
3. **`NfcServiceAndroid.cs`**: Implementación Android que:
   - Mantiene la conexión ISO-DEP abierta después de leer credencial
   - Envía comandos APDU de respuesta
   - Cierra conexión después de enviar respuesta
4. **`AccessNfcViewModel.cs`**: Orquesta el flujo:
   - Lee credencial → Valida con backend → Envía respuesta visual → Muestra resultado

### Flujo Implementado

```csharp
// En AccessNfcViewModel.cs - OnNfcTagDetected()

// 1. Detectar tag NFC (credencial digital)
// NfcServiceAndroid.cs lee credencial y mantiene conexión abierta

// 2. Validar acceso con backend
var validationResult = await ValidateAccessAsync(userId, controlPointId);

// 3. Crear evento de acceso
var accessEvent = await _accessEventApiService.CreateAccessEventAsync(request);

// 4. Enviar respuesta visual al celular con credencial
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

// 5. Mostrar resultado en pantalla del punto de control
ShowAccessResult(validationResult, tagId, eventDateTime);
```

### Código de Referencia (Ya Implementado)

#### NfcServiceAndroid.cs

// 1. Conectar al tag
var nfcTag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
var isoDep = IsoDep.Get(nfcTag);
isoDep.Connect();

try 
{
    // 2. Seleccionar AID
    byte[] selectAid = new byte[] { 
        0x00, 0xA4, 0x04, 0x00, 0x07, // SELECT APDU header
        0xF0, 0x39, 0x41, 0x48, 0x14, 0x81, 0x00 // AID
    };
    byte[] selectResponse = isoDep.Transceive(selectAid);
    
    // Verificar respuesta OK (90 00)
    if (selectResponse.Length >= 2 && 
        selectResponse[^2] == 0x90 && 
        selectResponse[^1] == 0x00)
    {
        // 3. Obtener datos de credencial
        byte[] getData = new byte[] { 0x00, 0xCA, 0x00, 0x00, 0x00 };
        byte[] dataResponse = isoDep.Transceive(getData);
        
        // Extraer datos (sin los últimos 2 bytes que son el status)
        string credentialData = Encoding.UTF8.GetString(dataResponse, 0, dataResponse.Length - 2);
        
        // Parsear: "CRED:123|USER:456"
        var parts = credentialData.Split('|');
        int credentialId = int.Parse(parts[0].Split(':')[1]);
        int userId = int.Parse(parts[1].Split(':')[1]);
        
        // 4. Validar credencial con tu lógica de negocio
        bool accessGranted = await ValidateAccess(credentialId, userId);
        
        // 5. Enviar respuesta visual al celular
        if (accessGranted)
        {
            await SendAccessGrantedAsync(isoDep, "✅ Acceso concedido");
        }
        else
        {
            await SendAccessDeniedAsync(isoDep, "❌ Acceso denegado");
        }
    }
}
finally
{
    isoDep.Close();
}
```

### Paso 2: Implementar métodos de respuesta

```csharp
private async Task SendAccessGrantedAsync(IsoDep isoDep, string message)
{
    try
    {
        // Construir comando ACCESS GRANTED
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] command = new byte[4 + messageBytes.Length];
        
        // APDU header: 00 AC 01 00
        command[0] = 0x00;  // CLA
        command[1] = 0xAC;  // INS - Access Control
        command[2] = 0x01;  // P1 - Granted
        command[3] = 0x00;  // P2
        
        // Agregar mensaje
        Array.Copy(messageBytes, 0, command, 4, messageBytes.Length);
        
        // Enviar comando
        byte[] response = isoDep.Transceive(command);
        
        System.Diagnostics.Debug.WriteLine("✅ ACCESS GRANTED sent successfully");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error sending ACCESS GRANTED: {ex.Message}");
    }
}

private async Task SendAccessDeniedAsync(IsoDep isoDep, string message)
{
    try
    {
        // Construir comando ACCESS DENIED
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] command = new byte[4 + messageBytes.Length];
        
        // APDU header: 00 AC 00 00
        command[0] = 0x00;  // CLA
        command[1] = 0xAC;  // INS - Access Control
        command[2] = 0x00;  // P1 - Denied
        command[3] = 0x00;  // P2
        
        // Agregar mensaje
        Array.Copy(messageBytes, 0, command, 4, messageBytes.Length);
        
        // Enviar comando
        byte[] response = isoDep.Transceive(command);
        
        System.Diagnostics.Debug.WriteLine("❌ ACCESS DENIED sent successfully");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error sending ACCESS DENIED: {ex.Message}");
    }
}
```

## 📱 Comportamiento en el Mobile (Credencial)

Cuando el punto de control envía un comando ACCESS GRANTED o ACCESS DENIED:

1. **El servicio HCE** (`NfcHostCardEmulationService`) detecta el comando
2. **Dispara el evento** `OnAccessResponseReceived` con un objeto `AccessResponse`
3. **El ViewModel** (`CredentialViewModel`) recibe el evento
4. **Muestra una alerta** al usuario con el resultado
5. **Actualiza el mensaje de estado** temporalmente

### Ejemplo de Alerta

**Acceso Concedido:**
```
┌─────────────────────────────┐
│  ✅ Acceso Concedido        │
│                             │
│  Acceso concedido           │
│                             │
│         [ OK ]              │
└─────────────────────────────┘
```

**Acceso Denegado:**
```
┌─────────────────────────────┐
│  ❌ Acceso Denegado         │
│                             │
│  Sin permisos para acceder  │
│                             │
│         [ OK ]              │
└─────────────────────────────┘
```

## 🔧 Consideraciones Técnicas

### Timeouts
- La comunicación NFC debe ser rápida (< 2 segundos)
- Validación de credencial debe ser eficiente
- Usar cache local si es posible

### Manejo de Errores
- Si el celular se aleja antes de recibir la respuesta, no habrá alerta
- Implementar logs para debugging
- Reintentos automáticos no son recomendados en NFC

### Seguridad
- Los mensajes no están encriptados en este nivel
- La seguridad viene de la validación en el backend
- No enviar información sensible en los mensajes de respuesta

## 🎯 Próximos Pasos

### ✅ Implementación Completada

Toda la funcionalidad ha sido implementada:

- ✅ Modelo `AccessResponse` en Mobile
- ✅ Evento `AccessResponseReceived` en `INfcCredentialService`
- ✅ Procesamiento de comandos APDU en `NfcHostCardEmulationService`
- ✅ Suscripción a eventos en `CredentialViewModel`
- ✅ Alertas visuales en el celular con credencial
- ✅ Métodos `SendAccessGrantedAsync` y `SendAccessDeniedAsync` en Mobile.AccessPoint
- ✅ Integración en `AccessNfcViewModel` con flujo completo
- ✅ Manejo de conexión ISO-DEP en `NfcServiceAndroid`

### 🧪 Pruebas Requeridas

Para probar la funcionalidad completa:

1. **Dos celulares Android con NFC**
   - Celular A: Ejecutar Mobile (app de credencial)
   - Celular B: Ejecutar Mobile.AccessPoint (punto de control)

2. **Proceso de prueba**
   ```
   1. En Celular A: Iniciar sesión y activar credencial
   2. En Celular B: Iniciar sesión como punto de control y empezar a escuchar
   3. Acercar Celular A a Celular B (back-to-back)
   4. Observar:
      - Celular B valida y muestra resultado
      - Celular A recibe alerta visual "✅ Acceso concedido" o "❌ Acceso denegado"
   ```

3. **Logs a verificar**
   - Mobile: "📩 Access response received: True/False - {Message}"
   - AccessPoint: "✅ Visual response sent to credential device successfully"

### 🔧 Ajustes Opcionales

1. **Personalizar mensajes**
   - Modificar texto en `AccessNfcViewModel.cs` líneas de `SendAccessGrantedAsync`/`SendAccessDeniedAsync`
   - Agregar información adicional (hora, ubicación, etc.)

2. **Mejorar UI**
   - Agregar animaciones en `CredentialViewModel.OnAccessResponseReceived`
   - Vibraciones o sonidos
   - Notificaciones persistentes

3. **Optimizar timeouts**
   - Ajustar tiempo de espera en comunicación NFC
   - Timeout de conexión ISO-DEP

## 🔒 Seguridad

- [Android NFC Documentation](https://developer.android.com/guide/topics/connectivity/nfc)
- [Host Card Emulation](https://developer.android.com/guide/topics/connectivity/nfc/hce)
- [ISO/IEC 7816-4 APDU](https://en.wikipedia.org/wiki/Smart_card_application_protocol_data_unit)
