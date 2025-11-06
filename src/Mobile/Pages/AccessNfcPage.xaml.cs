using Application.AccessEvents.DTOs;
using Microsoft.Extensions.Logging;
using Mobile.Services;

namespace Mobile.Pages;

public partial class AccessNfcPage : ContentPage
{
    private readonly INfcService _nfcService;
    private readonly IAccessEventApiService _accessEventService;
    private readonly ILogger<AccessNfcPage> _logger;
    private readonly int _currentUserId = 1; // TODO: Obtener del contexto de autenticación

    public AccessNfcPage(
        INfcService nfcService, 
        IAccessEventApiService accessEventService,
        ILogger<AccessNfcPage> logger)
    {
        InitializeComponent();
        _nfcService = nfcService;
        _accessEventService = accessEventService;
        _logger = logger;

        _nfcService.TagDetected += OnNfcTagDetected;

        UpdateNfcStatus();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateNfcStatus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _nfcService.StopListening();
    }

    private void UpdateNfcStatus()
    {
        NfcAvailableLabel.Text = _nfcService.IsAvailable ? "✓ Sí" : "✗ No";
        NfcAvailableLabel.TextColor = _nfcService.IsAvailable ? Colors.Green : Colors.Red;

        NfcEnabledLabel.Text = _nfcService.IsEnabled ? "✓ Sí" : "✗ No";
        NfcEnabledLabel.TextColor = _nfcService.IsEnabled ? Colors.Green : Colors.Red;

        if (!_nfcService.IsAvailable)
        {
            InstructionLabel.Text = "NFC no está disponible en este dispositivo";
            StartButton.IsEnabled = false;
        }
        else if (!_nfcService.IsEnabled)
        {
            InstructionLabel.Text = "Por favor, habilita NFC en la configuración del dispositivo";
            StartButton.IsEnabled = false;
        }
        else
        {
            StartButton.IsEnabled = true;
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        try
        {
            await _nfcService.StartListeningAsync();

            StartButton.IsVisible = false;
            StopButton.IsVisible = true;
            ScanningIndicator.IsRunning = true;
            ResultFrame.IsVisible = false;
            ControlPointFrame.IsVisible = false;

            StatusLabel.Text = "Escuchando...";
            InstructionLabel.Text = "Acerca tu tarjeta NFC al dispositivo";
            ScanFrame.BackgroundColor = Colors.LightGreen;

            _logger.LogInformation("NFC listening started");
        }
        catch (NotSupportedException)
        {
            await DisplayAlert("Error", "NFC no está soportado en este dispositivo", "OK");
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting NFC listening");
            await DisplayAlert("Error", $"Error al iniciar NFC: {ex.Message}", "OK");
        }
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        _nfcService.StopListening();

        StartButton.IsVisible = true;
        StopButton.IsVisible = false;
        ScanningIndicator.IsRunning = false;

        StatusLabel.Text = "Listo para Escanear";
        InstructionLabel.Text = "Acerca tu tarjeta o dispositivo NFC al lector";
        ScanFrame.BackgroundColor = Colors.LightBlue;

        _logger.LogInformation("NFC listening stopped");
    }

    private async void OnNfcTagDetected(object? sender, NfcTagDetectedEventArgs e)
    {
        _logger.LogInformation("NFC Tag detected in UI: ControlPoint {ControlPointId}, Tag {TagId}", 
            e.ControlPointId, e.TagId);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                // Mostrar información del punto de control
                ControlPointFrame.IsVisible = true;
                ControlPointLabel.Text = $"{e.ControlPointName} (ID: {e.ControlPointId})";

                // Mostrar validando
                StatusLabel.Text = "Validando...";
                ScanFrame.BackgroundColor = Colors.Orange;

                // Llamar al backend para crear el evento de acceso
                var request = new CreateAccessEventRequest
                {
                    UserId = _currentUserId,
                    ControlPointId = e.ControlPointId,
                    EventDateTime = DateTime.UtcNow,
                    Result = "Granted" // TODO: Backend should determine this based on access rules
                };

                _logger.LogInformation("Calling backend API to create access event");
                var result = await _accessEventService.CreateAccessEventAsync(request);

                // Determinar si el acceso fue concedido
                var granted = result.Result.Equals("Granted", StringComparison.OrdinalIgnoreCase);

                ShowResult(granted, result.ControlPoint.Name, e.TagId, result.EventDateTime);
                
                _logger.LogInformation("Access event created: {EventId}, Result: {Result}", 
                    result.Id, result.Result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de conectividad al procesar NFC tag");
                await DisplayAlert("Error de Conexión", 
                    $"No se pudo conectar con el servidor.\n\n" +
                    $"Detalles:\n{ex.Message}\n\n" +
                    $"Verifica que:\n" +
                    $"• Backend esté corriendo (192.168.1.6:5000)\n" +
                    $"• Dispositivo esté en la misma red WiFi", 
                    "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar NFC tag");
                await DisplayAlert("Error", $"Error al procesar tag NFC: {ex.Message}", "OK");
            }
        });
    }

    private void ShowResult(bool granted, string controlPointName, string tagId, DateTime eventDateTime)
    {
        ResultFrame.IsVisible = true;
        ScanFrame.BackgroundColor = granted ? Colors.LightGreen : Colors.LightCoral;
        StatusLabel.Text = granted ? "Acceso Permitido" : "Acceso Denegado";

        if (granted)
        {
            ResultFrame.BorderColor = Colors.Green;
            ResultIconLabel.Text = "✓";
            ResultIconLabel.TextColor = Colors.Green;
            ResultTitleLabel.Text = "Acceso Permitido";
            ResultTitleLabel.TextColor = Colors.Green;
            ResultMessageLabel.Text = "Bienvenido/a. Puedes ingresar.";
        }
        else
        {
            ResultFrame.BorderColor = Colors.Red;
            ResultIconLabel.Text = "✗";
            ResultIconLabel.TextColor = Colors.Red;
            ResultTitleLabel.Text = "Acceso Denegado";
            ResultTitleLabel.TextColor = Colors.Red;
            ResultMessageLabel.Text = "No tienes permisos para acceder a esta área.";
        }

        UserLabel.Text = $"Usuario {_currentUserId}";
        LocationLabel.Text = controlPointName;
        DateTimeLabel.Text = eventDateTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        TagIdLabel.Text = tagId;

        // Auto-detener después de mostrar resultado
        Task.Delay(5000).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (StopButton.IsVisible)
                {
                    OnStopClicked(this, EventArgs.Empty);
                }
            });
        });
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        // TODO: Navigate to history page
        await DisplayAlert("Historial", "Funcionalidad de historial próximamente", "OK");
    }
}
