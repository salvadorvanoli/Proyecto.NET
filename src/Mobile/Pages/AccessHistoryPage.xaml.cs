using Mobile.ViewModels;

namespace Mobile.Pages;

public partial class AccessHistoryPage : ContentPage
{
    private readonly AccessHistoryViewModel _viewModel;

    public AccessHistoryPage(AccessHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
