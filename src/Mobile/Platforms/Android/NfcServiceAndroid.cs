#if ANDROID
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Mobile.Services;

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
        await Task.Run(() =>
        {
            _currentActivity = Platform.CurrentActivity;
            if (_currentActivity == null)
                throw new InvalidOperationException("Cannot get current activity");

            _nfcAdapter = NfcAdapter.GetDefaultAdapter(_currentActivity);

            if (_nfcAdapter == null)
                throw new NotSupportedException("NFC is not supported on this device");

            if (!_nfcAdapter.IsEnabled)
                throw new InvalidOperationException("NFC is not enabled. Please enable NFC in device settings.");

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

            // Filtros para diferentes tipos de tags NFC
            _intentFilters = new[]
            {
                new IntentFilter(NfcAdapter.ActionNdefDiscovered),
                new IntentFilter(NfcAdapter.ActionTagDiscovered),
                new IntentFilter(NfcAdapter.ActionTechDiscovered)
            };

            // Tecnologías NFC que queremos detectar
            var techLists = new[]
            {
                new[] { Java.Lang.Class.FromType(typeof(Ndef)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcA)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcB)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcF)).Name },
                new[] { Java.Lang.Class.FromType(typeof(NfcV)).Name }
            };

            // Habilitar Foreground Dispatch
            _nfcAdapter.EnableForegroundDispatch(
                _currentActivity,
                _pendingIntent,
                _intentFilters,
                techLists
            );

            _logger.LogInformation("Android NFC listening started");
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

        // Intentar leer mensaje NDEF
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

        // Si no se pudo leer NDEF, usar el ID del tag como fallback
        if (controlPointId == 0)
        {
            _logger.LogWarning("Could not read NDEF data, using tag ID hash as fallback");
            controlPointId = (Math.Abs(tagId.GetHashCode()) % 6) + 1;
            controlPointName = $"Control Point {controlPointId}";
        }

        // Disparar evento
        var eventArgs = new NfcTagDetectedEventArgs
        {
            TagId = tagId,
            ControlPointId = controlPointId,
            ControlPointName = controlPointName
        };

        TagDetected?.Invoke(this, eventArgs);
    }
}
#endif
