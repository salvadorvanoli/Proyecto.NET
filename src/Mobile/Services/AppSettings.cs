using Mobile.Models;

namespace Mobile.Services;

/// <summary>
/// Application-wide settings and configuration
/// </summary>
public static class AppSettings
{
    private const string APP_MODE_KEY = "app_mode";
    private const string USER_ID_KEY = "user_id";
    private const string CREDENTIAL_ID_KEY = "credential_id";

    /// <summary>
    /// Gets or sets the current application mode
    /// </summary>
    public static AppMode CurrentMode
    {
        get
        {
            var modeString = Preferences.Get(APP_MODE_KEY, AppMode.ControlPoint.ToString());
            return Enum.Parse<AppMode>(modeString);
        }
        set => Preferences.Set(APP_MODE_KEY, value.ToString());
    }

    /// <summary>
    /// Gets or sets the current user ID (for Credential mode)
    /// </summary>
    public static int? UserId
    {
        get
        {
            var userId = Preferences.Get(USER_ID_KEY, -1);
            return userId > 0 ? userId : null;
        }
        set => Preferences.Set(USER_ID_KEY, value ?? -1);
    }

    /// <summary>
    /// Gets or sets the credential ID to emit via NFC (for Credential mode)
    /// </summary>
    public static int? CredentialId
    {
        get
        {
            var credId = Preferences.Get(CREDENTIAL_ID_KEY, -1);
            return credId > 0 ? credId : null;
        }
        set => Preferences.Set(CREDENTIAL_ID_KEY, value ?? -1);
    }

    /// <summary>
    /// Gets or sets the control point ID for this device (for ControlPoint mode)
    /// </summary>
    public static int? ControlPointId
    {
        get
        {
            var cpId = Preferences.Get("control_point_id", -1);
            return cpId > 0 ? cpId : null;
        }
        set => Preferences.Set("control_point_id", value ?? -1);
    }

    /// <summary>
    /// Clears all settings
    /// </summary>
    public static void Clear()
    {
        Preferences.Remove(APP_MODE_KEY);
        Preferences.Remove(USER_ID_KEY);
        Preferences.Remove(CREDENTIAL_ID_KEY);
    }
}
