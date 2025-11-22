using Mobile.Credential.ViewModels;

namespace Mobile.Credential.Pages;

public partial class CredentialPage : ContentPage
{
    public CredentialPage(CredentialViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

