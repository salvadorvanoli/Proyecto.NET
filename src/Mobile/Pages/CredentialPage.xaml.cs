using Mobile.ViewModels;

namespace Mobile.Pages;

public partial class CredentialPage : ContentPage
{
    private CredentialViewModel ViewModel => (CredentialViewModel)BindingContext;

    public CredentialPage(CredentialViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Subscribe to property changes to control button visibility
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsEmulating))
        {
            StartButton.IsVisible = !ViewModel.IsEmulating;
            StopButton.IsVisible = ViewModel.IsEmulating;
        }
    }
}
