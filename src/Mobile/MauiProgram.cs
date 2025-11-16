using Microsoft.Extensions.Logging;
using Mobile.Pages;
using Mobile.Services;
using Mobile.ViewModels;
using Mobile.Data;

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

		// Configure HttpClient for backend API
		builder.Services.AddHttpClient<IAccessEventApiService, AccessEventApiService>(client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.23:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});

		// Configure HttpClient for AccessRuleApiService
		builder.Services.AddHttpClient<AccessRuleApiService>(client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.23:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});

		// Configure HttpClient for AuthService
		builder.Services.AddHttpClient("AuthClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.23:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});
		builder.Services.AddSingleton<IAuthService, AuthService>();

		// Register services
		builder.Services.AddTransient<IMobileAccessRuleService, AccessRuleService>();
		
		// Register NFC service
		builder.Services.AddSingleton<INfcService, NfcService>();
		
		// Register NFC Credential service (HCE)
#if ANDROID
		builder.Services.AddSingleton<INfcCredentialService, Platforms.Android.Services.NfcCredentialService>();
#endif
		
		// Register database
		builder.Services.AddSingleton<ILocalDatabase, LocalDatabase>();
		
		// Register sync service
		builder.Services.AddSingleton<ISyncService, SyncService>();
		
		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<AccessNfcViewModel>();
		builder.Services.AddTransient<CredentialViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		
		// Register pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<AccessNfcPage>();
		builder.Services.AddTransient<CredentialPage>();
		builder.Services.AddTransient<SettingsPage>();

		var app = builder.Build();

		return app;
	}
}
