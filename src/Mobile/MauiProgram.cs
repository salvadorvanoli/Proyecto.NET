using Microsoft.Extensions.Logging;
using Mobile.Data;
using Mobile.Pages;
using Mobile.Services;
using Mobile.ViewModels;
using Mobile.Configuration;
using CommunityToolkit.Maui;
using System.Reflection;
using System.Text.Json;

namespace Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Cargar configuración desde appsettings.json
		var appSettings = LoadAppSettings();
		builder.Services.AddSingleton(appSettings);

		// Obtener BaseUrl de configuración (o usar default para desarrollo)
		var baseUrl = appSettings.ApiSettings.BaseUrl;
		if (string.IsNullOrEmpty(baseUrl))
		{
			// Default para desarrollo local
			baseUrl = "http://192.168.1.28:5000/";
			System.Diagnostics.Debug.WriteLine("⚠️ Usando BaseUrl por defecto para desarrollo");
		}
		
		var tenantId = appSettings.ApiSettings.TenantId;
		if (string.IsNullOrEmpty(tenantId))
		{
			tenantId = "1";
		}

		// Register JWT Token Handler
		builder.Services.AddTransient<JwtTokenHandler>();

		// Configurar HttpClient con seguridad mejorada
		builder.Services.AddHttpClient("AuthClient", client =>
		{
			client.BaseAddress = new Uri(baseUrl);
			client.Timeout = TimeSpan.FromSeconds(appSettings.ApiSettings.Timeout);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
			client.DefaultRequestHeaders.Add("User-Agent", "IndigoMobileApp/1.0");
		})
		.ConfigurePrimaryHttpMessageHandler(() => CreateSecureHttpHandler(appSettings));
		
		builder.Services.AddSingleton<IAuthService, AuthService>();
		
		// Register Navigation and Dialog Services for MVVM
		builder.Services.AddSingleton<INavigationService, NavigationService>();
		builder.Services.AddSingleton<IDialogService, DialogService>();
		
		// Configure HttpClient for UserService
		builder.Services.AddHttpClient("UserClient", client =>
		{
			client.BaseAddress = new Uri(baseUrl);
			client.Timeout = TimeSpan.FromSeconds(appSettings.ApiSettings.Timeout);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
			client.DefaultRequestHeaders.Add("User-Agent", "IndigoMobileApp/1.0");
		})
		.ConfigurePrimaryHttpMessageHandler(() => CreateSecureHttpHandler(appSettings))
		.AddHttpMessageHandler<JwtTokenHandler>();
		
		builder.Services.AddSingleton<IUserService, UserService>();
		
		// Configure HttpClient for AccessEventService
		builder.Services.AddHttpClient("AccessEventClient", client =>
		{
			client.BaseAddress = new Uri(baseUrl);
			client.Timeout = TimeSpan.FromSeconds(appSettings.ApiSettings.Timeout);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
			client.DefaultRequestHeaders.Add("User-Agent", "IndigoMobileApp/1.0");
		})
		.ConfigurePrimaryHttpMessageHandler(() => CreateSecureHttpHandler(appSettings))
		.AddHttpMessageHandler<JwtTokenHandler>();
		
		builder.Services.AddSingleton<IAccessEventService, AccessEventService>();
		
		// Configure HttpClient for BenefitService
		builder.Services.AddHttpClient("BenefitClient", client =>
		{
			client.BaseAddress = new Uri(baseUrl);
			client.Timeout = TimeSpan.FromSeconds(appSettings.ApiSettings.Timeout);
			client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
			client.DefaultRequestHeaders.Add("User-Agent", "IndigoMobileApp/1.0");
		})
		.ConfigurePrimaryHttpMessageHandler(() => CreateSecureHttpHandler(appSettings))
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
		builder.Services.AddSingleton<INotificationService, Platforms.Android.Services.NotificationService>();
#endif
		
		// Register Connectivity Monitor Service
		builder.Services.AddSingleton<IConnectivityMonitorService, ConnectivityMonitorService>();
		
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
	
	/// <summary>
	/// Carga configuración desde appsettings.json embebido
	/// </summary>
	private static AppSettings LoadAppSettings()
	{
		try
		{
			var assembly = Assembly.GetExecutingAssembly();
			using var stream = assembly.GetManifestResourceStream("Mobile.appsettings.json");
			
			if (stream != null)
			{
				using var reader = new StreamReader(stream);
				var json = reader.ReadToEnd();
				var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				
				if (settings != null)
				{
					System.Diagnostics.Debug.WriteLine($"✅ Configuración cargada desde appsettings.json");
					System.Diagnostics.Debug.WriteLine($"   BaseUrl: {settings.ApiSettings.BaseUrl}");
					return settings;
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"⚠️ Error cargando appsettings.json: {ex.Message}");
		}
		
		// Retornar configuración por defecto si falla
		System.Diagnostics.Debug.WriteLine("⚠️ Usando configuración por defecto");
		return new AppSettings();
	}
	
	/// <summary>
	/// Crea un HttpMessageHandler seguro con TLS 1.2+ y certificate pinning opcional
	/// </summary>
	private static HttpMessageHandler CreateSecureHttpHandler(AppSettings settings)
	{
#if ANDROID
		var handler = new Xamarin.Android.Net.AndroidMessageHandler
		{
			// Forzar TLS 1.2 o superior
			SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
			
			// Validar certificados del servidor
			ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
			{
				// En producción, implementar certificate pinning
				if (settings.Security.CertificatePinning.Enabled && cert != null)
				{
					var certHash = System.Security.Cryptography.SHA256.HashData(cert.RawData);
					var certPin = Convert.ToBase64String(certHash);
					
					if (!settings.Security.CertificatePinning.Pins.Contains(certPin))
					{
						System.Diagnostics.Debug.WriteLine($"❌ Certificate pinning failed!");
						return false;
					}
				}
				
				// Validación estándar de certificados
				return errors == System.Net.Security.SslPolicyErrors.None;
			}
		};
		
		return handler;
#else
		var handler = new HttpClientHandler
		{
			SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
		};
		
		return handler;
#endif
	}
}
