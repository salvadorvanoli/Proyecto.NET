using Android.Content;
using Android.Nfc;
using Android.Nfc.CardEmulators;
using Microsoft.Extensions.Logging;
using Mobile.Credential.Services;

namespace Mobile.Credential.Platforms.Android.Services;

/// <summary>
/// Android implementation of NFC credential service using Host Card Emulation (HCE)
/// </summary>
public class NfcCredentialService : INfcCredentialService
{
    private readonly ILogger<NfcCredentialService> _logger;
    private NfcAdapter? _nfcAdapter;
    private CardEmulation? _cardEmulation;
    private bool _isEmulating;

    public event EventHandler<AccessResponseEventArgs>? AccessResponseReceived;

    public NfcCredentialService(ILogger<NfcCredentialService> logger)
    {
        _logger = logger;
        InitializeNfc();
        
        // Suscribirse al evento estático del HCE service
        NfcHostCardEmulationService.AccessResponseReceived += OnAccessResponseReceived;
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

    private void OnAccessResponseReceived(object? sender, AccessResponseEventArgs e)
    {
        _logger.LogInformation("📱 Access response received in service: {Type}", 
            e.IsGranted ? "GRANTED" : "DENIED");
        
        // Reenviar el evento a través de la interfaz
        AccessResponseReceived?.Invoke(this, e);
    }
}
