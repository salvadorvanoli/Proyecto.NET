using Mobile.AccessPoint.Pages;
using Mobile.AccessPoint.Services;
using Mobile.AccessPoint.ViewModels;

namespace Mobile.AccessPoint;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App(IAuthService authService, LoginViewModel loginViewModel)
	{
		InitializeComponent();

		// Mostrar pantalla de login por defecto
		MainPage = new NavigationPage(new LoginPage(loginViewModel));
		
		// Intentar restaurar sesión en background
		Task.Run(async () => 
		{
			try
			{
				var currentUser = await authService.GetCurrentUserAsync();
				if (currentUser != null)
				{
					// Si hay sesión, navegar a AppShell en el UI thread
					MainThread.BeginInvokeOnMainThread(() =>
					{
						MainPage = new AppShell();
					});
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Init error: {ex.Message}");
			}
		});
	}
}
