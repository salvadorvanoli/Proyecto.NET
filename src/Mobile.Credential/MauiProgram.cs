using Microsoft.Extensions.Logging;
using Mobile.Credential.Pages;
using Mobile.Credential.Services;
using Mobile.Credential.ViewModels;

namespace Mobile.Credential;

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
			client.BaseAddress = new Uri("http://192.168.1.23:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});
		builder.Services.AddSingleton<IAuthService, AuthService>();

		// Register NFC Credential service (HCE)
#if ANDROID
		builder.Services.AddSingleton<INfcCredentialService, Platforms.Android.Services.NfcCredentialService>();
#endif
		
		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<CredentialViewModel>();
		
		// Register pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<CredentialPage>();

		return builder.Build();
	}
}
