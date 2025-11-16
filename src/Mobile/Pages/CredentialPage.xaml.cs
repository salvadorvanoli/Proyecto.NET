using Mobile.ViewModels;

namespace Mobile.Pages;

public partial class CredentialPage : ContentPage
{
    public CredentialPage(CredentialViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
