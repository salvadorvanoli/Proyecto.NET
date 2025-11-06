namespace Mobile.Services;

/// <summary>
/// Service interface for NFC operations.
/// </summary>
public interface INfcService
{
    /// <summary>
    /// Checks if NFC is available on the device.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Checks if NFC is enabled on the device.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Starts listening for NFC tags.
    /// </summary>
    Task StartListeningAsync();

    /// <summary>
    /// Stops listening for NFC tags.
    /// </summary>
    void StopListening();

    /// <summary>
    /// Event fired when an NFC tag is detected.
    /// </summary>
    event EventHandler<NfcTagDetectedEventArgs> TagDetected;
}

/// <summary>
/// Event args for NFC tag detection.
/// </summary>
public class NfcTagDetectedEventArgs : EventArgs
{
    public string TagId { get; set; } = string.Empty;
    public int ControlPointId { get; set; }
    public string ControlPointName { get; set; } = string.Empty;
}
