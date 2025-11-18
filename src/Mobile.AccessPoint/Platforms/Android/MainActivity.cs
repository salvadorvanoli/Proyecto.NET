using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Mobile.AccessPoint.Services;

namespace Mobile.AccessPoint;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        
        // Pasar el intent al servicio NFC para procesar el tag
        if (intent != null)
        {
            var nfcService = MauiApplication.Current?.Services.GetService<INfcService>() as NfcService;
            nfcService?.ProcessIntent(intent);
        }
    }
}
