using CommunityToolkit.Maui.Views;

namespace Mobile.Views;

public partial class AccessResultPopup : Popup
{
    public AccessResultPopup(bool wasGranted, string message)
    {
        InitializeComponent();
        
        if (wasGranted)
        {
            BindingContext = new
            {
                Icon = "✅",
                Title = "Acceso Concedido",
                Message = message,
                BackgroundColor = Colors.Green,
                TitleColor = Colors.Green
            };
        }
        else
        {
            BindingContext = new
            {
                Icon = "❌",
                Title = "Acceso Denegado",
                Message = message,
                BackgroundColor = Colors.Red,
                TitleColor = Colors.Red
            };
        }
        
        // Animación de entrada
        AnimateIcon();
    }
    
    private async void AnimateIcon()
    {
        IconFrame.Scale = 0;
        await IconFrame.ScaleTo(1.2, 200, Easing.CubicOut);
        await IconFrame.ScaleTo(1, 100, Easing.CubicIn);
    }
    
    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Close();
    }
}
