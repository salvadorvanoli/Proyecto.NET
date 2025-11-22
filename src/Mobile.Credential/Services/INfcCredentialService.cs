namespace Mobile.Credential.Services;

/// <summary>
/// Argumentos del evento de respuesta de acceso
/// </summary>
public class AccessResponseEventArgs : EventArgs
{
    public bool IsGranted { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Service interface for NFC Host Card Emulation (HCE)
/// Allows a device to emulate an NFC card/tag
/// </summary>
public interface INfcCredentialService
{
    /// <summary>
    /// Evento que se dispara cuando se recibe una respuesta de acceso del punto de control
    /// </summary>
    event EventHandler<AccessResponseEventArgs>? AccessResponseReceived;

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

