using Android.Content;
using Android.Hardware.Biometrics;
using Android.OS;
using Android.Runtime;
using Mobile.Services;
using JavaObject = Java.Lang.Object;
using Java.Lang;

namespace Mobile.Platforms.Android.Services;

public class BiometricAuthService : IBiometricAuthService
{
    public Task<bool> AuthenticateAsync(string title = "Autenticación requerida", string description = "Verifica tu identidad para continuar")
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();

        // Verificar que estamos en Android 9+ (API 28+)
        if (Build.VERSION.SdkInt < BuildVersionCodes.P)
        {
            System.Diagnostics.Debug.WriteLine("[BiometricAuth] Biometric API requires Android 9+");
            taskCompletionSource.SetResult(false);
            return taskCompletionSource.Task;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var activity = Platform.CurrentActivity;
                if (activity == null)
                {
                    taskCompletionSource.SetResult(false);
                    return;
                }

                var biometricManager = activity.GetSystemService(Context.BiometricService) as BiometricManager;
                if (biometricManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("[BiometricAuth] BiometricManager not available");
                    taskCompletionSource.SetResult(false);
                    return;
                }

                // Verificar si el dispositivo tiene biometría disponible
                var canAuthenticate = biometricManager.CanAuthenticate();
                if (canAuthenticate != 0) // 0 = BIOMETRIC_SUCCESS
                {
                    System.Diagnostics.Debug.WriteLine($"[BiometricAuth] Biometric not available: {canAuthenticate}");
                    taskCompletionSource.SetResult(false);
                    return;
                }

                // Crear el callback de autenticación
                var authCallback = new BiometricAuthCallback(
                    onSuccess: () => taskCompletionSource.TrySetResult(true),
                    onError: (errorCode, errString) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"[BiometricAuth] Error: {errorCode} - {errString}");
                        taskCompletionSource.TrySetResult(false);
                    },
                    onFailed: () =>
                    {
                        System.Diagnostics.Debug.WriteLine("[BiometricAuth] Authentication failed");
                        taskCompletionSource.TrySetResult(false);
                    }
                );

                // Crear el prompt de autenticación
                var promptBuilder = new BiometricPrompt.Builder(activity)
                    .SetTitle(title)
                    .SetDescription(description)
                    .SetNegativeButton("Cancelar", activity.MainExecutor, new DialogClickListener());

                var prompt = promptBuilder.Build();

                // Crear el objeto crypto (requerido en algunas versiones de Android)
                var cancellationSignal = new global::Android.OS.CancellationSignal();

                // Autenticar
                prompt.Authenticate(cancellationSignal, activity.MainExecutor, authCallback);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricAuth] Exception: {ex.Message}");
                taskCompletionSource.SetResult(false);
            }
        });

        return taskCompletionSource.Task;
    }

    private class BiometricAuthCallback : BiometricPrompt.AuthenticationCallback
    {
        private readonly Action _onSuccess;
        private readonly Action<int, ICharSequence> _onError;
        private readonly Action _onFailed;

        public BiometricAuthCallback(Action onSuccess, Action<int, ICharSequence> onError, Action onFailed)
        {
            _onSuccess = onSuccess;
            _onError = onError;
            _onFailed = onFailed;
        }

        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult? result)
        {
            base.OnAuthenticationSucceeded(result);
            _onSuccess?.Invoke();
        }

        public override void OnAuthenticationError([GeneratedEnum] BiometricErrorCode errorCode, ICharSequence? errString)
        {
            base.OnAuthenticationError(errorCode, errString);
            _onError?.Invoke((int)errorCode, errString ?? new Java.Lang.String("Unknown error"));
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
            _onFailed?.Invoke();
        }
    }

    private class DialogClickListener : JavaObject, IDialogInterfaceOnClickListener
    {
        public void OnClick(IDialogInterface? dialog, int which)
        {
            dialog?.Dismiss();
        }
    }
}
