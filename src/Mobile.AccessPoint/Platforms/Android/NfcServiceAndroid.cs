#if ANDROID
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Mobile.AccessPoint.Services;

/// <summary>
/// Implementación nativa de NFC para Android usando Android.Nfc APIs.
/// Esta clase extiende NfcService con funcionalidad específica de Android.
/// </summary>
public partial class NfcService
{
    private NfcAdapter? _nfcAdapter;
    private Activity? _currentActivity;
    private PendingIntent? _pendingIntent;
    private IntentFilter[]? _intentFilters;
    private IsoDep? _currentIsoDep; // Mantener referencia al tag actual para respuestas

    private partial bool GetIsAvailableAndroid()
    {
        try
        {
            _currentActivity = Platform.CurrentActivity;
            if (_currentActivity == null)
                return false;

            _nfcAdapter ??= NfcAdapter.GetDefaultAdapter(_currentActivity);
            return _nfcAdapter != null;
        }
        catch
        {
            return false;
        }
    }

    private partial bool GetIsEnabledAndroid()
    {
        try
        {
            _currentActivity = Platform.CurrentActivity;
            if (_currentActivity == null)
                return false;

            _nfcAdapter ??= NfcAdapter.GetDefaultAdapter(_currentActivity);
            return _nfcAdapter?.IsEnabled ?? false;
        }
        catch
        {
            return false;
        }
    }

    private partial async Task StartListeningAndroidAsync()
    {
        // Ejecutar en el Main Thread para asegurar que Platform.CurrentActivity esté disponible
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _currentActivity = Platform.CurrentActivity;
            if (_currentActivity == null)
            {
                _logger.LogError("Cannot get current activity");
                throw new InvalidOperationException("Cannot get current activity");
            }

            _nfcAdapter = NfcAdapter.GetDefaultAdapter(_currentActivity);

            if (_nfcAdapter == null)
            {
                _logger.LogError("NFC not supported on this device");
                throw new NotSupportedException("NFC is not supported on this device");
            }

            if (!_nfcAdapter.IsEnabled)
            {
                _logger.LogError("NFC is not enabled");
                throw new InvalidOperationException("NFC is not enabled. Please enable NFC in device settings.");
            }

            _logger.LogInformation("Creating PendingIntent for NFC...");

            // Crear PendingIntent para capturar tags NFC
            var intent = new Intent(_currentActivity, _currentActivity.GetType())
                .AddFlags(ActivityFlags.SingleTop);

            // Compatibilidad con diferentes versiones de Android
#pragma warning disable CA1416
            var flags = PendingIntentFlags.UpdateCurrent;
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                flags |= PendingIntentFlags.Mutable;
            }
#pragma warning restore CA1416

            _pendingIntent = PendingIntent.GetActivity(
                _currentActivity, 
                0, 
                intent,
                flags
            );

            _logger.LogInformation("PendingIntent created: {IsNull}", _pendingIntent == null ? "NULL" : "OK");

            // Filtros para diferentes tipos de tags NFC
            _intentFilters = new[]
            {
                new IntentFilter(NfcAdapter.ActionNdefDiscovered),
                new IntentFilter(NfcAdapter.ActionTagDiscovered),
                new IntentFilter(NfcAdapter.ActionTechDiscovered)
            };

            _logger.LogInformation("Intent filters created: {Count}", _intentFilters.Length);

            // Tecnologías NFC que queremos detectar - incluyendo ISO-DEP para HCE
            var techLists = new[]
            {
                new[] { Java.Lang.Class.FromType(typeof(IsoDep)).Name },
                new[] { Java.Lang.Class.FromType(typeof(Ndef)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcA)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcB)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcF)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcV)).Name }
            };

            _logger.LogInformation("Tech lists created: {Count}", techLists.Length);
            _logger.LogInformation("Enabling Foreground Dispatch...");

            // Habilitar Foreground Dispatch
            _nfcAdapter.EnableForegroundDispatch(
                _currentActivity,
                _pendingIntent,
                _intentFilters,
                techLists
            );

            _logger.LogInformation("✅ Android NFC listening started successfully");
        });
    }

    private partial void StopListeningAndroid()
    {
        if (_nfcAdapter == null || _currentActivity == null)
            return;

        try
        {
            _nfcAdapter.DisableForegroundDispatch(_currentActivity);
            _logger.LogInformation("Android NFC listening stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping NFC listening");
        }
    }

    /// <summary>
    /// Procesa un Intent de Android que contiene datos de un tag NFC.
    /// Llama este método desde MainActivity.OnNewIntent().
    /// </summary>
    public void ProcessIntent(Intent? intent)
    {
        if (intent == null || !_isListening)
            return;

        var action = intent.Action;
        if (action != NfcAdapter.ActionNdefDiscovered && 
            action != NfcAdapter.ActionTagDiscovered &&
            action != NfcAdapter.ActionTechDiscovered)
            return;

        Tag? tag = null;
        
        // Usar API compatible con versiones antiguas y nuevas de Android
#pragma warning disable CA1422, CS0618
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
        {
            tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag, Java.Lang.Class.FromType(typeof(Tag))) as Tag;
        }
        else
        {
            tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
        }
#pragma warning restore CA1422, CS0618
        
        if (tag == null)
            return;

        _logger.LogInformation("NFC tag detected: {TagId}", BitConverter.ToString(tag.GetId() ?? Array.Empty<byte>()));

        try
        {
            ProcessNfcTag(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing NFC tag");
        }
    }

    /// <summary>
    /// Procesa el tag NFC y extrae la información del punto de control.
    /// </summary>
    private void ProcessNfcTag(Tag tag)
    {
        var tagId = BitConverter.ToString(tag.GetId() ?? Array.Empty<byte>()).Replace("-", "");
        
        int controlPointId = 0;
        string controlPointName = "Unknown";
        int? userId = null;
        int? credentialId = null;

        // PRIORIDAD 1: Intentar leer HCE (ISO-DEP) para credenciales digitales
        var isoDep = IsoDep.Get(tag);
        if (isoDep != null)
        {
            try
            {
                _logger.LogInformation("ISO-DEP tag found, attempting to connect...");
                isoDep.Connect();
                
                // Aumentar timeout para mantener conexión durante validación backend
                isoDep.Timeout = 10000; // 10 segundos
                
                _logger.LogInformation("✓ Connected to ISO-DEP tag (HCE) with 10s timeout");
                
                // Guardar referencia para poder enviar respuestas después
                _currentIsoDep = isoDep;

                // SELECT AID command (debe coincidir con el AID del servicio HCE)
                byte[] selectAid = { 0x00, 0xA4, 0x04, 0x00, 0x07, 0xF0, 0x39, 0x41, 0x48, 0x14, 0x81, 0x00, 0x00 };
                _logger.LogInformation("Sending SELECT AID command...");
                var response = isoDep.Transceive(selectAid);
                _logger.LogInformation("SELECT AID response length: {Length}", response?.Length ?? 0);
                
                if (response != null && response.Length >= 2)
                {
                    // Verificar status code 90 00 (success)
                    var sw1 = response[response.Length - 2];
                    var sw2 = response[response.Length - 1];
                    
                    if (sw1 == 0x90 && sw2 == 0x00)
                    {
                        _logger.LogInformation("AID selected successfully, requesting data");
                        
                        // GET DATA command
                        byte[] getData = { 0x00, 0xCA, 0x00, 0x00, 0x00 };
                        var dataResponse = isoDep.Transceive(getData);
                        
                        _logger.LogInformation("========================================");
                        _logger.LogInformation("📡 ISO-DEP HCE RESPONSE RECEIVED");
                        _logger.LogInformation("Response length: {Length}", dataResponse?.Length ?? 0);
                        _logger.LogInformation("========================================");
                        
                        if (dataResponse != null && dataResponse.Length > 2)
                        {
                            // Quitar los últimos 2 bytes (status code)
                            var dataLength = dataResponse.Length - 2;
                            var credentialData = Encoding.UTF8.GetString(dataResponse, 0, dataLength);
                            
                            _logger.LogInformation("========================================");
                            _logger.LogInformation("📨 CREDENTIAL DATA RECEIVED FROM HCE");
                            _logger.LogInformation("Raw data: '{Data}'", credentialData);
                            _logger.LogInformation("Data length: {Length}", credentialData.Length);
                            _logger.LogInformation("========================================");
                            
                            // Parsear "CRED:1|USER:2"
                            var parts = credentialData.Split('|');
                            _logger.LogInformation("Parsing {Count} parts from payload", parts.Length);
                            
                            foreach (var part in parts)
                            {
                                _logger.LogInformation("  Parsing part: '{Part}'", part);
                                var keyValue = part.Split(':');
                                if (keyValue.Length == 2)
                                {
                                    if (keyValue[0] == "CRED" && int.TryParse(keyValue[1], out var cId))
                                    {
                                        credentialId = cId;
                                        _logger.LogInformation("  ✅ Found CredentialId: {CredentialId}", cId);
                                    }
                                    else if (keyValue[0] == "USER" && int.TryParse(keyValue[1], out var uId))
                                    {
                                        userId = uId;
                                        _logger.LogInformation("  ✅ Found UserId: {UserId}", uId);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("  ⚠️ Unknown key or invalid value: {Key}={Value}", keyValue[0], keyValue[1]);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("  ⚠️ Invalid part format (expected KEY:VALUE): '{Part}'", part);
                                }
                            }
                            
                            _logger.LogInformation("========================================");
                            _logger.LogInformation("📊 PARSING RESULT:");
                            _logger.LogInformation("  UserId: {UserId}", userId?.ToString() ?? "NULL");
                            _logger.LogInformation("  CredentialId: {CredentialId}", credentialId?.ToString() ?? "NULL");
                            _logger.LogInformation("========================================");
                            
                            if (credentialId.HasValue && userId.HasValue)
                            {
                                _logger.LogInformation("✅ Digital credential SUCCESSFULLY detected - UserId: {UserId}, CredentialId: {CredentialId}", 
                                    userId, credentialId);
                                
                                // Para credenciales digitales, el controlPointId se obtiene del ViewModel
                                // No usamos AppSettings porque AccessPoint siempre online no necesita configuración persistente
                                controlPointId = 1; // El ViewModel lo sobreescribirá con el ID configurado
                                controlPointName = $"Control Point {controlPointId}";
                            }
                            else
                            {
                                _logger.LogWarning("❌ Digital credential INCOMPLETE - UserId: {UserId}, CredentialId: {CredentialId}", 
                                    userId?.ToString() ?? "NULL", credentialId?.ToString() ?? "NULL");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("❌ NO RESPONSE or response too short from HCE");
                        }
                    }
                }
                
                // NO cerrar isoDep aquí - lo necesitamos para enviar la respuesta
                // isoDep.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading ISO-DEP HCE data");
                CloseCurrentIsoDep();
            }
        }

        // PRIORIDAD 2: Intentar leer mensaje NDEF (tags NFC tradicionales)
        if (!userId.HasValue || !credentialId.HasValue)
        {
            var ndef = Ndef.Get(tag);
            if (ndef != null)
            {
                try
                {
                    ndef.Connect();
                    var ndefMessage = ndef.NdefMessage;
                    
                    if (ndefMessage != null)
                    {
                        var records = ndefMessage.GetRecords();
                        if (records != null && records.Length > 0)
                        {
                            var record = records[0];
                            var payload = record?.GetPayload();
                            
                            if (payload != null && payload.Length > 0)
                            {
                                // Los registros de texto NDEF tienen el formato:
                                // [0] = Status byte (indica idioma)
                                // [1..n] = Código de idioma (ej: "en")
                                // [n+1..] = Texto
                                
                                // Saltar el byte de status y el código de idioma
                                var statusByte = payload[0];
                                var languageCodeLength = statusByte & 0x3F; // Los 6 bits inferiores
                                var textOffset = 1 + languageCodeLength;
                                
                                if (payload.Length > textOffset)
                                {
                                    var text = Encoding.UTF8.GetString(payload, textOffset, payload.Length - textOffset);
                                    _logger.LogInformation("NDEF text payload: {Text}", text);

                                    // Parsear "CONTROL_POINT:3:Entrada Estacionamiento"
                                    var parts = text.Split(':');
                                    if (parts.Length >= 3 && parts[0] == "CONTROL_POINT")
                                    {
                                        if (int.TryParse(parts[1], out var cpId))
                                        {
                                            controlPointId = cpId;
                                            controlPointName = parts[2];
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    ndef.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading NDEF message");
                }
            }
        }

        // Si no se pudo leer NDEF ni HCE, usar el ID del tag como fallback
        if (controlPointId == 0)
        {
            _logger.LogWarning("Could not read NDEF or HCE data, using tag ID hash as fallback");
            controlPointId = (Math.Abs(tagId.GetHashCode()) % 6) + 1;
            controlPointName = $"Control Point {controlPointId}";
        }

        // Disparar evento con todos los datos
        var eventArgs = new NfcTagDetectedEventArgs
        {
            TagId = tagId,
            ControlPointId = controlPointId,
            ControlPointName = controlPointName,
            UserId = userId,
            CredentialId = credentialId
        };

        TagDetected?.Invoke(this, eventArgs);
    }
    
    private partial async Task<bool> SendAccessGrantedAndroidAsync(string message)
    {
        if (_currentIsoDep == null)
        {
            _logger.LogWarning("Cannot send ACCESS GRANTED - No ISO-DEP reference");
            return false;
        }

        try
        {
            // Verificar y reconectar si es necesario
            if (!_currentIsoDep.IsConnected)
            {
                _logger.LogWarning("ISO-DEP connection lost, attempting to reconnect...");
                _currentIsoDep.Connect();
                _currentIsoDep.Timeout = 10000;
                _logger.LogInformation("Reconnected to ISO-DEP");
            }
            
            _logger.LogInformation("📤 Sending ACCESS GRANTED to credential device: {Message}", message);
            
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
            
            // Enviar comando con timeout generoso
            var response = await Task.Run(() => _currentIsoDep.Transceive(command));
            
            if (response != null && response.Length >= 2)
            {
                var sw1 = response[response.Length - 2];
                var sw2 = response[response.Length - 1];
                
                if (sw1 == 0x90 && sw2 == 0x00)
                {
                    _logger.LogInformation("✅ ACCESS GRANTED sent successfully");
                    
                    // Esperar un poco antes de cerrar para que el HCE procese
                    await Task.Delay(500);
                    CloseCurrentIsoDep();
                    return true;
                }
            }
            
            _logger.LogWarning("⚠️ ACCESS GRANTED sent but received unexpected response");
            CloseCurrentIsoDep();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ACCESS GRANTED: {Message}", ex.Message);
            CloseCurrentIsoDep();
            return false;
        }
    }
    
    private partial async Task<bool> SendAccessDeniedAndroidAsync(string message)
    {
        if (_currentIsoDep == null)
        {
            _logger.LogWarning("Cannot send ACCESS DENIED - No ISO-DEP reference");
            return false;
        }

        try
        {
            // Verificar y reconectar si es necesario
            if (!_currentIsoDep.IsConnected)
            {
                _logger.LogWarning("ISO-DEP connection lost, attempting to reconnect...");
                _currentIsoDep.Connect();
                _currentIsoDep.Timeout = 10000;
                _logger.LogInformation("Reconnected to ISO-DEP");
            }
            
            _logger.LogInformation("📤 Sending ACCESS DENIED to credential device: {Message}", message);
            
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
            
            // Enviar comando con timeout generoso
            var response = await Task.Run(() => _currentIsoDep.Transceive(command));
            
            if (response != null && response.Length >= 2)
            {
                var sw1 = response[response.Length - 2];
                var sw2 = response[response.Length - 1];
                
                if (sw1 == 0x90 && sw2 == 0x00)
                {
                    _logger.LogInformation("✅ ACCESS DENIED sent successfully");
                    
                    // Esperar un poco antes de cerrar para que el HCE procese
                    await Task.Delay(500);
                    CloseCurrentIsoDep();
                    return true;
                }
            }
            
            _logger.LogWarning("⚠️ ACCESS DENIED sent but received unexpected response");
            CloseCurrentIsoDep();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ACCESS DENIED: {Message}", ex.Message);
            CloseCurrentIsoDep();
            return false;
        }
    }
    
    private void CloseCurrentIsoDep()
    {
        try
        {
            if (_currentIsoDep != null && _currentIsoDep.IsConnected)
            {
                _currentIsoDep.Close();
                _logger.LogInformation("ISO-DEP connection closed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing ISO-DEP connection");
        }
        finally
        {
            _currentIsoDep = null;
        }
    }
}
#endif
