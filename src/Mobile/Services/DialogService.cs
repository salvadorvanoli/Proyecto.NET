namespace Mobile.Services;

/// <summary>
/// Implementation of IDialogService using Shell dialogs.
/// </summary>
public class DialogService : IDialogService
{
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        await Shell.Current.DisplayAlert(title, message, cancel);
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        return await Shell.Current.DisplayAlert(title, message, accept, cancel);
    }
}
