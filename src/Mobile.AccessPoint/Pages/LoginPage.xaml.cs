using Mobile.AccessPoint.ViewModels;

namespace Mobile.AccessPoint.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

