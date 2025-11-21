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

		// Register JWT Token Handler
		builder.Services.AddTransient<JwtTokenHandler>();

		// Configure HttpClient for AuthService (no requiere JWT porque es para login)
		builder.Services.AddHttpClient("AuthClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.2:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});
		builder.Services.AddSingleton<IAuthService, AuthService>();
		
		// Configure HttpClient for UserService
		builder.Services.AddHttpClient("UserClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.2:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		})
		.AddHttpMessageHandler<JwtTokenHandler>();
		builder.Services.AddSingleton<IUserService, UserService>();
		
		// Configure HttpClient for AccessEventService
		builder.Services.AddHttpClient("AccessEventClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.2:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		})
		.AddHttpMessageHandler<JwtTokenHandler>();
		builder.Services.AddSingleton<IAccessEventService, AccessEventService>();
		
		// Configure HttpClient for BenefitService
		builder.Services.AddHttpClient("BenefitClient", client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.2:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		})
		.AddHttpMessageHandler<JwtTokenHandler>();
		builder.Services.AddSingleton<IBenefitService, BenefitService>();
		
		// Register SQLite Database
		builder.Services.AddSingleton<ILocalDatabase, LocalDatabase>();
		
		// Register SyncService
		builder.Services.AddSingleton<ISyncService, SyncService>();
		
		// Register NFC Credential service (HCE) - for emitting credential
#if ANDROID
		builder.Services.AddSingleton<INfcCredentialService, Platforms.Android.Services.NfcCredentialService>();
		builder.Services.AddSingleton<IBiometricAuthService, Platforms.Android.Services.BiometricAuthService>();
#endif
		
		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<CredentialViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<AccessHistoryViewModel>();
		builder.Services.AddTransient<RedeemBenefitViewModel>();
		
		// Register pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<CredentialPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<AccessHistoryPage>();
		builder.Services.AddTransient<RedeemBenefitPage>();

		var app = builder.Build();

		return app;
	}
}
