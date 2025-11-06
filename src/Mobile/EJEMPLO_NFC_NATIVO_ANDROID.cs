// 📱 EJEMPLO: Implementación NFC Real para Android
// 
// Este archivo muestra cómo implementar NFC nativo en Android.
// NO lo agregues al proyecto todavía - es solo un EJEMPLO de referencia.
// 
// Cuando estés listo para NFC real:
// 1. Crea Mobile/Platforms/Android/NfcServiceAndroid.cs
// 2. Copia y adapta este código
// 3. Modifica NfcService.cs para usar partial classes
// 4. Prueba en dispositivo físico Android

#if ANDROID
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Microsoft.Maui.ApplicationModel;
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

    /// <summary>
    /// Verifica si el dispositivo tiene hardware NFC.
    /// </summary>
    public override bool IsAvailable
    {
        get
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
    }

    /// <summary>
    /// Verifica si NFC está habilitado en la configuración del dispositivo.
    /// </summary>
    public override bool IsEnabled
    {
        get
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
    }

    /// <summary>
    /// Inicia la escucha de tags NFC usando Foreground Dispatch.
    /// </summary>
    public override async Task StartListeningAsync()
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

            _pendingIntent = PendingIntent.GetActivity(
                _currentActivity, 
                0, 
                intent,
                PendingIntentFlags.Mutable | PendingIntentFlags.UpdateCurrent
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

            _isListening = true;
            _logger.LogInformation("Android NFC listening started");
        });
    }

    /// <summary>
    /// Detiene la escucha de tags NFC.
    /// </summary>
    public override void StopListening()
    {
        if (!_isListening || _nfcAdapter == null || _currentActivity == null)
            return;

        try
        {
            _nfcAdapter.DisableForegroundDispatch(_currentActivity);
            _isListening = false;
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

        var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
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
                
                if (ndefMessage != null && ndefMessage.GetRecords().Length > 0)
                {
                    var record = ndefMessage.GetRecords()[0];
                    var payload = record.GetPayload();
                    
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
            controlPointName = GetControlPointName(controlPointId);
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

/// <summary>
/// IMPORTANTE: También necesitas modificar MainActivity.cs
/// </summary>
/*
// En Mobile/Platforms/Android/MainActivity.cs

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Mobile.Services;

namespace Mobile;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    LaunchMode = LaunchMode.SingleTop  // IMPORTANTE para NFC
)]
[IntentFilter(
    new[] { Android.Nfc.NfcAdapter.ActionNdefDiscovered },
    Categories = new[] { Intent.CategoryDefault }
)]
public class MainActivity : MauiAppCompatActivity
{
    private INfcService? _nfcService;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Obtener el servicio NFC
        _nfcService = IPlatformApplication.Current?.Services.GetService<INfcService>();
    }

    protected override void OnResume()
    {
        base.OnResume();
        
        // Procesar intent si viene de NFC
        if (_nfcService is NfcService nfcService)
        {
            nfcService.ProcessIntent(Intent);
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        
        // Este método se llama cuando se detecta un tag NFC
        // mientras la app está en foreground
        if (_nfcService is NfcService nfcService)
        {
            nfcService.ProcessIntent(intent);
        }
    }

    protected override void OnPause()
    {
        base.OnPause();
        
        // El servicio maneja el stop en OnDisappearing de la página
    }
}
*/
#endif

// ═══════════════════════════════════════════════════════════════════
// PASOS PARA IMPLEMENTAR NFC REAL:
// ═══════════════════════════════════════════════════════════════════
//
// 1. Modificar NfcService.cs para usar partial class:
//    public partial class NfcService : INfcService
//
// 2. Crear Mobile/Platforms/Android/NfcServiceAndroid.cs
//    - Copiar el código de arriba
//
// 3. Modificar Mobile/Platforms/Android/MainActivity.cs
//    - Agregar LaunchMode = LaunchMode.SingleTop
//    - Agregar IntentFilter para NDEF
//    - Implementar OnNewIntent
//
// 4. Verificar AndroidManifest.xml tenga:
//    <uses-permission android:name="android.permission.NFC" />
//    <uses-feature android:name="android.hardware.nfc" android:required="false" />
//
// 5. Probar en dispositivo físico Android con tag NFC programado
//
// ═══════════════════════════════════════════════════════════════════