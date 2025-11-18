namespace Mobile.Credential.Services;

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

