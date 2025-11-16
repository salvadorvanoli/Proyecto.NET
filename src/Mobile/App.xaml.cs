using Mobile.Data;
using Mobile.Pages;
using Mobile.Services;

namespace Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App(ILocalDatabase database, IAuthService authService)
	{
		InitializeComponent();

		// Mostrar pantalla de login por defecto (no crashea)
		MainPage = new NavigationPage(new LoginPage(new ViewModels.LoginViewModel(authService)));
		
		// Intentar restaurar sesión en background
		Task.Run(async () => 
		{
			try
			{
				await database.InitializeDatabaseAsync();
				
				// Intentar restaurar sesión
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
				System.Diagnostics.Debug.WriteLine($"Init error: {ex.Message}\n{ex.StackTrace}");
			}
		});
	}
}
