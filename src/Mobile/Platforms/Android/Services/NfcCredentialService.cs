using Android.Content;
using Android.Nfc;
using Android.Nfc.CardEmulators;
using Microsoft.Extensions.Logging;
using Mobile.Services;

namespace Mobile.Platforms.Android.Services;

/// <summary>
/// Android implementation of NFC credential service using Host Card Emulation (HCE)
/// </summary>
public class NfcCredentialService : INfcCredentialService
{
    private readonly ILogger<NfcCredentialService> _logger;
    private NfcAdapter? _nfcAdapter;
    private CardEmulation? _cardEmulation;
    private bool _isEmulating;

    public event EventHandler<AccessResponse>? AccessResponseReceived;

    public NfcCredentialService(ILogger<NfcCredentialService> logger)
    {
        _logger = logger;
        InitializeNfc();
        
        _logger.LogInformation("🔔 NfcCredentialService constructor - Suscribiendo a evento HCE");
        // Subscribe to HCE service events
        NfcHostCardEmulationService.OnAccessResponseReceived += HandleAccessResponse;
        _logger.LogInformation("✅ Suscripción completada");
    }

    private void HandleAccessResponse(object? sender, AccessResponse response)
    {
        _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        _logger.LogInformation("📩 HandleAccessResponse CALLED in NfcCredentialService");
        _logger.LogInformation("   Access response: {AccessGranted} - {Message}", 
            response.AccessGranted, response.Message);
        
        _logger.LogInformation("🔔 Invocando AccessResponseReceived event...");
        _logger.LogInformation("   Event is null? {IsNull}", AccessResponseReceived == null);
        _logger.LogInformation("   Subscriber count: {Count}", AccessResponseReceived?.GetInvocationList()?.Length ?? 0);
        
        AccessResponseReceived?.Invoke(this, response);
        
        _logger.LogInformation("✅ Event invoked from NfcCredentialService");
        _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    public bool IsHceAvailable
    {
        get
        {
            if (_nfcAdapter == null || _cardEmulation == null)
                return false;

            var componentName = new ComponentName(
                global::Android.App.Application.Context,
                Java.Lang.Class.FromType(typeof(NfcHostCardEmulationService)));

            return _nfcAdapter.IsEnabled && 
                   (_cardEmulation.IsDefaultServiceForCategory(componentName, CardEmulation.CategoryPayment) ||
                    _cardEmulation.IsDefaultServiceForCategory(componentName, CardEmulation.CategoryOther));
        }
    }

    public int? CredentialId { get; set; }
    public int? UserId { get; set; }
    public bool IsEmulating => _isEmulating;

    public Task StartEmulatingAsync()
    {
        try
        {
            if (!CredentialId.HasValue || !UserId.HasValue)
            {
                throw new InvalidOperationException("CredentialId and UserId must be set before starting emulation");
            }

            _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _logger.LogInformation("🔵 Starting NFC credential emulation");
            _logger.LogInformation("   CredentialId: {CredentialId}", CredentialId);
            _logger.LogInformation("   UserId: {UserId}", UserId);

            // Set the credential data in the HCE service
            NfcHostCardEmulationService.SetCredential(CredentialId, UserId);
            _logger.LogInformation("✅ Credential data set in HCE service");

            // Set this app as the preferred payment service
            if (_cardEmulation != null && _nfcAdapter != null && _nfcAdapter.IsEnabled)
            {
                var componentName = new ComponentName(
                    global::Android.App.Application.Context,
                    Java.Lang.Class.FromType(typeof(NfcHostCardEmulationService)));

                bool isDefault = _cardEmulation.IsDefaultServiceForCategory(componentName, CardEmulation.CategoryPayment);
                _logger.LogInformation("   Is default payment service: {IsDefault}", isDefault);

                if (!isDefault)
                {
                    _logger.LogWarning("⚠️ Esta app NO está configurada como servicio de pago predeterminado");
                    _logger.LogWarning("   El usuario debe configurarla manualmente en Ajustes > NFC > App de pago");
                }

                // The HCE service is now ready to respond when another device reads this device
                _isEmulating = true;
                _logger.LogInformation("✅ NFC credential emulation started successfully");
                _logger.LogInformation("🔔 Device is now emulating digital credential");
                _logger.LogInformation("   Waiting for NFC reader to connect...");
                _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            else
            {
                throw new InvalidOperationException("NFC is not available or not enabled");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting NFC credential emulation");
            throw;
        }
    }

    public void StopEmulating()
    {
        try
        {
            _logger.LogInformation("Stopping NFC credential emulation");
            
            NfcHostCardEmulationService.SetCredential(null, null);
            // NO desuscribirse del evento - mantener la suscripción activa
            // NfcHostCardEmulationService.OnAccessResponseReceived -= HandleAccessResponse;
            _isEmulating = false;
            
            _logger.LogInformation("NFC credential emulation stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping NFC credential emulation");
        }
    }

    private void InitializeNfc()
    {
        try
        {
            var context = global::Android.App.Application.Context;
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(context);
            
            if (_nfcAdapter != null)
            {
                _cardEmulation = CardEmulation.GetInstance(_nfcAdapter);
                _logger.LogInformation("NFC adapter initialized - Enabled: {Enabled}", _nfcAdapter.IsEnabled);
            }
            else
            {
                _logger.LogWarning("NFC adapter not available on this device");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing NFC adapter");
        }
    }
}
