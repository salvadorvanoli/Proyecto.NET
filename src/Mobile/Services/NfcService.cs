using Microsoft.Extensions.Logging;

namespace Mobile.Services;

/// <summary>
/// Service for NFC operations - Platform-specific implementation required.
/// This is a base implementation that can be extended with platform-specific code.
/// </summary>
public partial class NfcService : INfcService
{
    private readonly ILogger<NfcService> _logger;
    private bool _isListening;

    public event EventHandler<NfcTagDetectedEventArgs>? TagDetected;

    // TODO: Implementar con APIs nativas de plataforma
    // Android: Use Android.Nfc namespace
    // iOS: Use CoreNFC framework
    public virtual bool IsAvailable
    {
        get
        {
#if ANDROID
            return GetIsAvailableAndroid();
#elif IOS
            return true; // TODO: Verificar con CoreNFC availability
#else
            return false;
#endif
        }
    }
    
    public virtual bool IsEnabled
    {
        get
        {
#if ANDROID
            return GetIsEnabledAndroid();
#elif IOS
            return true; // TODO: iOS siempre tiene NFC habilitado si está disponible
#else
            return false;
#endif
        }
    }

    public NfcService(ILogger<NfcService> logger)
    {
        _logger = logger;
    }

    public virtual async Task StartListeningAsync()
    {
        if (!IsAvailable)
        {
            _logger.LogWarning("NFC is not supported on this device");
            throw new NotSupportedException("NFC is not supported on this device");
        }

        if (!IsEnabled)
        {
            _logger.LogWarning("NFC is not enabled on this device");
            throw new InvalidOperationException("NFC is not enabled. Please enable NFC in device settings.");
        }

        if (_isListening)
        {
            _logger.LogInformation("Already listening for NFC tags");
            return;
        }

        _logger.LogInformation("Starting NFC listening");
        
#if ANDROID
        await StartListeningAndroidAsync();
        _isListening = true;
#elif IOS
        // TODO: iOS implementation
        _isListening = true;
#else
        // Simulación solo en plataformas sin implementación nativa
        _isListening = true;
        SimulateNfcDetection();
#endif
    }

    public virtual void StopListening()
    {
        if (!_isListening)
            return;

        _logger.LogInformation("Stopping NFC listening");

#if ANDROID
        StopListeningAndroid();
#elif IOS
        // TODO: iOS implementation
#endif

        _isListening = false;
    }

#if !ANDROID && !IOS
    // Simulación temporal para testing sin hardware NFC
    private async void SimulateNfcDetection()
    {
        await Task.Delay(3000); // Simular espera

        if (!_isListening)
            return;

        _logger.LogInformation("Simulating NFC tag detection");

        var random = new Random();
        var controlPointId = random.Next(1, 7); // 1-6
        
        var eventArgs = new NfcTagDetectedEventArgs
        {
            TagId = $"SIMULATED-{Guid.NewGuid():N}",
            ControlPointId = controlPointId,
            ControlPointName = GetControlPointName(controlPointId)
        };

        TagDetected?.Invoke(this, eventArgs);
    }

    private string GetControlPointName(int id)
    {
        return id switch
        {
            1 => "Entrada Principal",
            2 => "Salida Principal",
            3 => "Entrada Estacionamiento",
            4 => "Salida Estacionamiento",
            5 => "Área Restringida",
            6 => "Sala de Servidores",
            _ => $"Control Point {id}"
        };
    }
#endif

#if ANDROID
    // Métodos específicos de Android (implementados en NfcServiceAndroid.cs)
    private partial bool GetIsAvailableAndroid();
    private partial bool GetIsEnabledAndroid();
    private partial Task StartListeningAndroidAsync();
    private partial void StopListeningAndroid();
#endif
}
