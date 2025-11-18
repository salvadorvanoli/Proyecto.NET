using Mobile.AccessPoint.ViewModels;

namespace Mobile.AccessPoint.Pages;

public partial class AccessNfcPage : ContentPage
{
    private readonly AccessNfcViewModel _viewModel;

    public AccessNfcPage(AccessNfcViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}

