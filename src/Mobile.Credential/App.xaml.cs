using Mobile.Credential.Pages;
using Mobile.Credential.Services;
using Mobile.Credential.ViewModels;

namespace Mobile.Credential;

public partial class App : Application
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
