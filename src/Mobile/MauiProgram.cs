using Microsoft.Extensions.Logging;
using Mobile.Pages;
using Mobile.Services;

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
		// IP de tu PC en la red local (obtenida con ipconfig)
		builder.Services.AddHttpClient<IAccessEventApiService, AccessEventApiService>(client =>
		{
			client.BaseAddress = new Uri("http://192.168.1.6:5000/");
			client.Timeout = TimeSpan.FromSeconds(30);
			
			// Header temporal para testing (X-Tenant-Id hardcodeado)
			client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
		});

		// Register NFC service
		builder.Services.AddSingleton<INfcService, NfcService>();
		
		// Register pages
		builder.Services.AddTransient<AccessNfcPage>();

		return builder.Build();
	}
}
