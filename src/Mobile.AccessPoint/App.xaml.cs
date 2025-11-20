using Mobile.AccessPoint.Pages;
using Mobile.AccessPoint.Services;
using Mobile.AccessPoint.ViewModels;

namespace Mobile.AccessPoint;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App(IAuthService authService, LoginViewModel loginViewModel)
	{
		InitializeComponent();

		// IR DIRECTO A LA PANTALLA DE ESCANEO NFC (sin login)
		MainPage = new AppShell();
		
		System.Diagnostics.Debug.WriteLine("✅ AccessPoint iniciado sin login - modo debug");
	}
}
