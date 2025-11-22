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

		// Inicializar con AppShell
		MainPage = new AppShell();
		
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
						
						MainThread.BeginInvokeOnMainThread(async () =>
						{
							await Shell.Current.DisplayAlert(
								"Sesión Cerrada", 
								"Tu cuenta ha sido desactivada. Por favor contacta al administrador.", 
								"OK");
							await Shell.Current.GoToAsync("//LoginPage");
						});
						return;
					}
					
					// Si hay sesión y usuario está activo, navegar a CredentialPage
					MainThread.BeginInvokeOnMainThread(async () =>
					{
						await Shell.Current.GoToAsync("//CredentialPage");
					});
					
					// Sincronizar eventos pendientes en background
					_ = Task.Run(async () => await _syncService.SyncPendingEventsAsync());
				}
				else
				{
					// No hay sesión - ir a login
					MainThread.BeginInvokeOnMainThread(async () =>
					{
						await Shell.Current.GoToAsync("//LoginPage");
					});
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
