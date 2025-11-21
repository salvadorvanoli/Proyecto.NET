using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.CardEmulators;
using Android.OS;
using System.Text;

namespace Mobile.Platforms.Android.Services;

/// <summary>
/// Android Host Card Emulation service
/// This service runs when another NFC reader tries to communicate with this device
/// </summary>
public class NfcHostCardEmulationService : HostApduService
{
    // AID (Application ID) for our NFC application
    // This must match the AID in apduservice.xml
    private static readonly byte[] AID = { 0xF0, 0x39, 0x41, 0x48, 0x14, 0x81, 0x00 };
    
    // APDU command codes
    private static readonly byte[] SELECT_APDU_HEADER = { 0x00, 0xA4, 0x04, 0x00 };
    private static readonly byte[] GET_DATA_APDU = { 0x00, 0xCA, 0x00, 0x00, 0x00 };
    private static readonly byte[] ACCESS_GRANTED_APDU = { 0x00, 0xAC, 0x01, 0x00 }; // Access Control: Granted
    private static readonly byte[] ACCESS_DENIED_APDU = { 0x00, 0xAC, 0x00, 0x00 };  // Access Control: Denied
    
    // Response codes
    private static readonly byte[] SELECT_OK_SW = { 0x90, 0x00 };
    private static readonly byte[] UNKNOWN_CMD_SW = { 0x00, 0x00 };
    private static readonly byte[] SELECT_FAILED_SW = { 0x6A, 0x82 };

    private static int? _credentialId;
    private static int? _userId;
    
    // Event to notify the app about access responses
    public static event EventHandler<Mobile.Services.AccessResponse>? OnAccessResponseReceived;

    public static void SetCredential(int? credentialId, int? userId)
    {
        System.Diagnostics.Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        System.Diagnostics.Debug.WriteLine($"🟢 NFC HCE: SetCredential CALLED");
        System.Diagnostics.Debug.WriteLine($"   OLD CredentialId: {_credentialId}");
        System.Diagnostics.Debug.WriteLine($"   OLD UserId: {_userId}");
        System.Diagnostics.Debug.WriteLine($"   NEW CredentialId: {credentialId}");
        System.Diagnostics.Debug.WriteLine($"   NEW UserId: {userId}");
        
        _credentialId = credentialId;
        _userId = userId;
        
        System.Diagnostics.Debug.WriteLine($"✅ NFC HCE: Credential SET successfully");
        System.Diagnostics.Debug.WriteLine($"   CURRENT CredentialId: {_credentialId}");
        System.Diagnostics.Debug.WriteLine($"   CURRENT UserId: {_userId}");
        System.Diagnostics.Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
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
            System.Diagnostics.Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            System.Diagnostics.Debug.WriteLine("📤 NFC HCE: GET DATA command received");
            System.Diagnostics.Debug.WriteLine($"   CredentialId.HasValue: {_credentialId.HasValue}");
            System.Diagnostics.Debug.WriteLine($"   UserId.HasValue: {_userId.HasValue}");
            System.Diagnostics.Debug.WriteLine($"   CredentialId: {_credentialId}");
            System.Diagnostics.Debug.WriteLine($"   UserId: {_userId}");
            
            if (_credentialId.HasValue && _userId.HasValue)
            {
                // Format: "CRED:{credentialId}|USER:{userId}"
                string credentialData = $"CRED:{_credentialId.Value}|USER:{_userId.Value}";
                System.Diagnostics.Debug.WriteLine($"🔨 Creating credential data string: '{credentialData}'");
                
                byte[] dataBytes = Encoding.UTF8.GetBytes(credentialData);
                System.Diagnostics.Debug.WriteLine($"   Data bytes length: {dataBytes.Length}");
                
                // Append success status code
                byte[] response = new byte[dataBytes.Length + 2];
                Array.Copy(dataBytes, 0, response, 0, dataBytes.Length);
                Array.Copy(SELECT_OK_SW, 0, response, dataBytes.Length, 2);
                
                System.Diagnostics.Debug.WriteLine($"✅ NFC HCE: Sending credential data: {credentialData}");
                System.Diagnostics.Debug.WriteLine($"   Total response length: {response.Length}");
                System.Diagnostics.Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return response;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ NFC HCE: No credential configured - Failing");
                return SELECT_FAILED_SW;
            }
        }
        
        // Check if it's an ACCESS GRANTED command from the control point
        if (IsAccessGrantedApdu(commandApdu))
        {
            System.Diagnostics.Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            System.Diagnostics.Debug.WriteLine("✅ NFC HCE: ACCESS GRANTED received from control point");
            
            // Extract message if present
            string message = "Acceso concedido";
            if (commandApdu.Length > ACCESS_GRANTED_APDU.Length)
            {
                try
                {
                    byte[] messageBytes = new byte[commandApdu.Length - ACCESS_GRANTED_APDU.Length];
                    Array.Copy(commandApdu, ACCESS_GRANTED_APDU.Length, messageBytes, 0, messageBytes.Length);
                    message = Encoding.UTF8.GetString(messageBytes).TrimEnd('\0');
                    System.Diagnostics.Debug.WriteLine($"   Extracted message: '{message}'");
                }
                catch (Exception ex)
                { 
                    System.Diagnostics.Debug.WriteLine($"   Error extracting message: {ex.Message}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"🔔 Invoking OnAccessResponseReceived event...");
            System.Diagnostics.Debug.WriteLine($"   Event is null? {OnAccessResponseReceived == null}");
            System.Diagnostics.Debug.WriteLine($"   Subscriber count: {OnAccessResponseReceived?.GetInvocationList()?.Length ?? 0}");
            
            // Notify the app
            var response = new Mobile.Services.AccessResponse
            {
                AccessGranted = true,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            
            OnAccessResponseReceived?.Invoke(null, response);
            System.Diagnostics.Debug.WriteLine("✅ Event invoked!");
            System.Diagnostics.Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            return SELECT_OK_SW;
        }
        
        // Check if it's an ACCESS DENIED command from the control point
        if (IsAccessDeniedApdu(commandApdu))
        {
            System.Diagnostics.Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            System.Diagnostics.Debug.WriteLine("❌ NFC HCE: ACCESS DENIED received from control point");
            
            // Extract message if present
            string message = "Acceso denegado";
            if (commandApdu.Length > ACCESS_DENIED_APDU.Length)
            {
                try
                {
                    byte[] messageBytes = new byte[commandApdu.Length - ACCESS_DENIED_APDU.Length];
                    Array.Copy(commandApdu, ACCESS_DENIED_APDU.Length, messageBytes, 0, messageBytes.Length);
                    message = Encoding.UTF8.GetString(messageBytes).TrimEnd('\0');
                    System.Diagnostics.Debug.WriteLine($"   Extracted message: '{message}'");
                }
                catch (Exception ex)
                { 
                    System.Diagnostics.Debug.WriteLine($"   Error extracting message: {ex.Message}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"🔔 Invoking OnAccessResponseReceived event...");
            System.Diagnostics.Debug.WriteLine($"   Event is null? {OnAccessResponseReceived == null}");
            System.Diagnostics.Debug.WriteLine($"   Subscriber count: {OnAccessResponseReceived?.GetInvocationList()?.Length ?? 0}");
            
            // Notify the app
            var response = new Mobile.Services.AccessResponse
            {
                AccessGranted = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            
            OnAccessResponseReceived?.Invoke(null, response);
            System.Diagnostics.Debug.WriteLine("✅ Event invoked!");
            System.Diagnostics.Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
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
    
    private bool IsAccessGrantedApdu(byte[] apdu)
    {
        if (apdu.Length < ACCESS_GRANTED_APDU.Length)
            return false;

        for (int i = 0; i < ACCESS_GRANTED_APDU.Length; i++)
        {
            if (apdu[i] != ACCESS_GRANTED_APDU[i])
                return false;
        }

        return true;
    }
    
    private bool IsAccessDeniedApdu(byte[] apdu)
    {
        if (apdu.Length < ACCESS_DENIED_APDU.Length)
            return false;

        for (int i = 0; i < ACCESS_DENIED_APDU.Length; i++)
        {
            if (apdu[i] != ACCESS_DENIED_APDU[i])
                return false;
        }

        return true;
    }
}
