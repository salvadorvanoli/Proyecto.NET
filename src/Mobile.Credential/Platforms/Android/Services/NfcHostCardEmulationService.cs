using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.CardEmulators;
using Android.OS;
using System.Text;
using Mobile.Credential.Services;

namespace Mobile.Credential.Platforms.Android.Services;

/// <summary>
/// Android Host Card Emulation service
/// This service runs when another NFC reader tries to communicate with this device
/// </summary>
public class NfcHostCardEmulationService : HostApduService
{
    // Evento estático para notificar respuestas de acceso al UI
    public static event EventHandler<AccessResponseEventArgs>? AccessResponseReceived;

    // AID (Application ID) for our NFC application
    // This must match the AID in apduservice.xml
    private static readonly byte[] AID = { 0xF0, 0x39, 0x41, 0x48, 0x14, 0x81, 0x00 };
    
    // APDU command codes
    private static readonly byte[] SELECT_APDU_HEADER = { 0x00, 0xA4, 0x04, 0x00 };
    private static readonly byte[] GET_DATA_APDU = { 0x00, 0xCA, 0x00, 0x00, 0x00 };
    
    // Response codes
    private static readonly byte[] SELECT_OK_SW = { 0x90, 0x00 };
    private static readonly byte[] UNKNOWN_CMD_SW = { 0x00, 0x00 };
    private static readonly byte[] SELECT_FAILED_SW = { 0x6A, 0x82 };

    private static int? _credentialId;
    private static int? _userId;

    public static void SetCredential(int? credentialId, int? userId)
    {
        _credentialId = credentialId;
        _userId = userId;
    }

    public override byte[]? ProcessCommandApdu(byte[]? commandApdu, Bundle? extras)
    {
        System.Diagnostics.Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        System.Diagnostics.Debug.WriteLine($"🔵 NFC HCE: ProcessCommandApdu LLAMADO");
        System.Diagnostics.Debug.WriteLine($"   Command APDU Length: {commandApdu?.Length ?? 0}");
        System.Diagnostics.Debug.WriteLine($"   CredentialId: {_credentialId}, UserId: {_userId}");
        
        if (commandApdu == null || commandApdu.Length < 4)
        {
            System.Diagnostics.Debug.WriteLine("❌ NFC HCE: Invalid APDU (too short)");
            return UNKNOWN_CMD_SW;
        }

        System.Diagnostics.Debug.WriteLine($"   APDU: {BitConverter.ToString(commandApdu)}");

        // Check if it's a SELECT command for our AID
        if (IsSelectAidApdu(commandApdu))
        {
            System.Diagnostics.Debug.WriteLine("✅ NFC HCE: SELECT AID command received - Responding OK");
            return SELECT_OK_SW;
        }

        // Check if it's a GET DATA command
        if (IsGetDataApdu(commandApdu))
        {
            System.Diagnostics.Debug.WriteLine("📤 NFC HCE: GET DATA command received");
            
            if (_credentialId.HasValue && _userId.HasValue)
            {
                // Format: "CRED:{credentialId}|USER:{userId}"
                string credentialData = $"CRED:{_credentialId.Value}|USER:{_userId.Value}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(credentialData);
                
                // Append success status code
                byte[] response = new byte[dataBytes.Length + 2];
                Array.Copy(dataBytes, 0, response, 0, dataBytes.Length);
                Array.Copy(SELECT_OK_SW, 0, response, dataBytes.Length, 2);
                
                System.Diagnostics.Debug.WriteLine($"✅ NFC HCE: Sending credential data: {credentialData}");
                System.Diagnostics.Debug.WriteLine($"   Response length: {response.Length}");
                return response;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ NFC HCE: No credential configured - Failing");
                return SELECT_FAILED_SW;
            }
        }

        // Check if it's an ACCESS CONTROL response from AccessPoint
        // ACCESS GRANTED: 00 AC 01 00 [message]
        // ACCESS DENIED:  00 AC 00 00 [message]
        if (commandApdu.Length >= 4 && 
            commandApdu[0] == 0x00 && 
            commandApdu[1] == 0xAC)
        {
            bool isGranted = commandApdu[2] == 0x01;
            string message = "Respuesta recibida";
            
            if (commandApdu.Length > 4)
            {
                try
                {
                    message = Encoding.UTF8.GetString(commandApdu, 4, commandApdu.Length - 4);
                }
                catch
                {
                    message = isGranted ? "Acceso concedido" : "Acceso denegado";
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            System.Diagnostics.Debug.WriteLine($"🎯 NFC HCE: ACCESS RESPONSE RECEIVED");
            System.Diagnostics.Debug.WriteLine($"   Type: {(isGranted ? "✅ GRANTED" : "❌ DENIED")}");
            System.Diagnostics.Debug.WriteLine($"   Message: {message}");
            System.Diagnostics.Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            // Disparar evento para notificar al UI
            try
            {
                AccessResponseReceived?.Invoke(null, new AccessResponseEventArgs
                {
                    IsGranted = isGranted,
                    Message = message
                });
                
                System.Diagnostics.Debug.WriteLine("✅ Event fired to UI");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error firing event: {ex.Message}");
            }
            
            return SELECT_OK_SW;
        }

        System.Diagnostics.Debug.WriteLine("❓ NFC HCE: Unknown command");
        return UNKNOWN_CMD_SW;
    }

    public override void OnDeactivated(DeactivationReason reason)
    {
        System.Diagnostics.Debug.WriteLine($"NFC HCE: Deactivated - Reason: {reason}");
    }

    private bool IsSelectAidApdu(byte[] apdu)
    {
        if (apdu.Length < 4 + SELECT_APDU_HEADER.Length)
            return false;

        for (int i = 0; i < SELECT_APDU_HEADER.Length; i++)
        {
            if (apdu[i] != SELECT_APDU_HEADER[i])
                return false;
        }

        return true;
    }

    private bool IsGetDataApdu(byte[] apdu)
    {
        if (apdu.Length < GET_DATA_APDU.Length)
            return false;

        for (int i = 0; i < GET_DATA_APDU.Length; i++)
        {
            if (apdu[i] != GET_DATA_APDU[i])
                return false;
        }

        return true;
    }
}
