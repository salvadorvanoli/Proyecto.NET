using Microsoft.Extensions.Logging;
using Mobile.AccessPoint.Pages;
using Mobile.AccessPoint.Services;
using Mobile.AccessPoint.ViewModels;

namespace Mobile.AccessPoint;

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
			client.BaseAddress = new Uri("http://192.168.1.5:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});

		// Configure HttpClient for AccessRuleApiService
		builder.Services.AddHttpClient<AccessRuleApiService>(client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.5:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});

		// Configure HttpClient for AuthService
		builder.Services.AddHttpClient("AuthClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.5:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});
		builder.Services.AddSingleton<IAuthService, AuthService>();

		// Register services (solo online - no database local)
		builder.Services.AddSingleton<INfcService, NfcService>();
		
		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<AccessNfcViewModel>();
		
		// Register pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<AccessNfcPage>();

		return builder.Build();
	}
}
