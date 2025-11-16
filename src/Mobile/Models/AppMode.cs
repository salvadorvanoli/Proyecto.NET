namespace Mobile.Models;

/// <summary>
/// Defines the operational mode of the application
/// </summary>
public enum AppMode
{
    /// <summary>
    /// Device acts as an NFC credential (emits credential via HCE)
    /// User's phone that presents the digital credential
    /// </summary>
    Credential,

    /// <summary>
    /// Device acts as a control point (reads NFC and validates)
    /// Fixed phone/tablet at entry points that validates access
    /// </summary>
    ControlPoint
}
