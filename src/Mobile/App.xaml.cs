using Mobile.Pages;
using Mobile.Services;

namespace Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
	private readonly ISyncService _syncService;
	private readonly IAuthService _authService;
	private readonly IConnectivityMonitorService _connectivityMonitor;
	
	public App(IAuthService authService, ISyncService syncService, IConnectivityMonitorService connectivityMonitor)
	{
		InitializeComponent();

		_authService = authService;
		_syncService = syncService;
		_connectivityMonitor = connectivityMonitor;

		// Iniciar monitoreo de conectividad
		_connectivityMonitor.StartMonitoring();

		// Mostrar pantalla de login por defecto
		MainPage = new NavigationPage(new LoginPage(new ViewModels.LoginViewModel(authService)));
		
		// Intentar restaurar sesión en background
		Task.Run(async () => 
		{
			try
			{
				// Intentar restaurar sesión
				var currentUser = await authService.GetCurrentUserAsync();
				if (currentUser != null)
				{
					// Verificar si el usuario sigue activo cuando hay conectividad
					var isActive = await _syncService.CheckUserStatusAsync();
					
					if (!isActive)
					{
						// Usuario desactivado - hacer logout
						await _authService.LogoutAsync();
						
						MainThread.BeginInvokeOnMainThread(() =>
						{
							MainPage?.DisplayAlert(
								"Sesión Cerrada", 
								"Tu cuenta ha sido desactivada. Por favor contacta al administrador.", 
								"OK");
						});
						return;
					}
					
					// Si hay sesión y usuario está activo, navegar a AppShell
					MainThread.BeginInvokeOnMainThread(() =>
					{
						MainPage = new AppShell();
					});
					
					// Sincronizar eventos pendientes en background
					_ = Task.Run(async () => await _syncService.SyncPendingEventsAsync());
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Init error: {ex.Message}\n{ex.StackTrace}");
			}
		});
		
		// Monitorear cambios de conectividad para auto-sincronización
		Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
	}
	
	private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
	{
		if (e.NetworkAccess == NetworkAccess.Internet)
		{
			// Conectividad restaurada - sincronizar eventos pendientes
			await _syncService.SyncPendingEventsAsync();
		}
	}
}
