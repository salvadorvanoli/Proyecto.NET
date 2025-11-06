using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Mobile.Services;

namespace Mobile;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density
)]
[IntentFilter(
    new[] { Android.Nfc.NfcAdapter.ActionNdefDiscovered },
    Categories = new[] { Intent.CategoryDefault }
)]
public class MainActivity : MauiAppCompatActivity
{
    private INfcService? _nfcService;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Obtener el servicio NFC
        _nfcService = IPlatformApplication.Current?.Services.GetService<INfcService>();
    }

    protected override void OnResume()
    {
        base.OnResume();
        
        // Procesar intent si viene de NFC
        if (_nfcService is NfcService nfcService)
        {
            nfcService.ProcessIntent(Intent);
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        
        // Este método se llama cuando se detecta un tag NFC
        // mientras la app está en foreground
        if (_nfcService is NfcService nfcService && intent != null)
        {
            nfcService.ProcessIntent(intent);
        }
    }
}
