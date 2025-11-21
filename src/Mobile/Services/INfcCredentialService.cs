namespace Mobile.Services;

/// <summary>
/// Response from the access control point
/// </summary>
public class AccessResponse
{
    public bool AccessGranted { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int? ControlPointId { get; set; }
    public string? ControlPointName { get; set; }
}

/// <summary>
/// Service interface for NFC Host Card Emulation (HCE)
/// Allows a device to emulate an NFC card/tag
/// </summary>
public interface INfcCredentialService
{
    /// <summary>
    /// Checks if HCE is available on the device
    /// </summary>
    bool IsHceAvailable { get; }

    /// <summary>
    /// Gets or sets the credential ID to be emitted via NFC
    /// </summary>
    int? CredentialId { get; set; }

    /// <summary>
    /// Gets or sets the user ID associated with the credential
    /// </summary>
    int? UserId { get; set; }

    /// <summary>
    /// Event raised when the access control point sends a response
    /// </summary>
    event EventHandler<AccessResponse>? AccessResponseReceived;

    /// <summary>
    /// Starts HCE service to emit credential via NFC
    /// </summary>
    Task StartEmulatingAsync();

    /// <summary>
    /// Stops HCE service
    /// </summary>
    void StopEmulating();

    /// <summary>
    /// Checks if the service is currently emulating
    /// </summary>
    bool IsEmulating { get; }
}
