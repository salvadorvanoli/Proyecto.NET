using CommunityToolkit.Maui.Views;

namespace Mobile.Popups;

public partial class AccessResultPopup : Popup
{
    public AccessResultPopup(bool isGranted, string message)
    {
        InitializeComponent();
        
        if (isGranted)
        {
            IconLabel.Text = "✅";
            TitleLabel.Text = "Acceso Concedido";
            TitleLabel.TextColor = Colors.Green;
        }
        else
        {
            IconLabel.Text = "⛔";
            TitleLabel.Text = "Acceso Denegado";
            TitleLabel.TextColor = Colors.Red;
        }
        
        MessageLabel.Text = message;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
