using Mobile.ViewModels;
using Mobile.Models;

namespace Mobile.Pages;

public partial class SettingsPage : ContentPage
{
    private SettingsViewModel ViewModel => (SettingsViewModel)BindingContext;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnCredentialModeClicked(object sender, EventArgs e)
    {
        ViewModel.SelectedMode = AppMode.Credential;
    }

    private void OnControlPointModeClicked(object sender, EventArgs e)
    {
        ViewModel.SelectedMode = AppMode.ControlPoint;
    }
}
