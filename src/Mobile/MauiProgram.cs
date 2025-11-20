using Microsoft.Extensions.Logging;
using Mobile.Data;
using Mobile.Pages;
using Mobile.Services;
using Mobile.ViewModels;

namespace Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Configure HttpClient for AuthService
		builder.Services.AddHttpClient("AuthClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.28:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});
		builder.Services.AddSingleton<IAuthService, AuthService>();
		
		// Configure HttpClient for UserService
		builder.Services.AddHttpClient("UserClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.28:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});
		builder.Services.AddSingleton<IUserService, UserService>();
		
		// Configure HttpClient for AccessEventService
		builder.Services.AddHttpClient("AccessEventClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.28:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});
		builder.Services.AddSingleton<IAccessEventService, AccessEventService>();
		
		// Register SQLite Database
		builder.Services.AddSingleton<ILocalDatabase, LocalDatabase>();
		
		// Register SyncService
		builder.Services.AddSingleton<ISyncService, SyncService>();
		
		// Register NFC Credential service (HCE) - for emitting credential
#if ANDROID
		builder.Services.AddSingleton<INfcCredentialService, Platforms.Android.Services.NfcCredentialService>();
#endif
		
		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<CredentialViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<AccessHistoryViewModel>();
		
		// Register pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<CredentialPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<AccessHistoryPage>();

		var app = builder.Build();

		return app;
	}
}
