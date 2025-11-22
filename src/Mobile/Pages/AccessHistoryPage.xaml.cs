using Mobile.ViewModels;

namespace Mobile.Pages;

public partial class AccessHistoryPage : ContentPage
{
    private AccessHistoryViewModel ViewModel => (AccessHistoryViewModel)BindingContext;

    public AccessHistoryPage(AccessHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Siempre refrescar al entrar a la pestaña
        await ViewModel.RefreshEventsAsync();
    }
}
