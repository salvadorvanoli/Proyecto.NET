namespace Mobile.Services;

/// <summary>
/// Service for displaying dialogs in MVVM pattern.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Displays an alert dialog.
    /// </summary>
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Displays a confirmation dialog.
    /// </summary>
    Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No");
}
