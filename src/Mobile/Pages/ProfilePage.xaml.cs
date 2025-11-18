using Mobile.ViewModels;

namespace Mobile.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            if (_viewModel != null)
            {
                await _viewModel.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ProfilePage.OnAppearing: {ex.Message}");
            await DisplayAlert("Error", $"No se pudo cargar el perfil: {ex.Message}", "OK");
        }
    }
}
