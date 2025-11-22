using System.Windows.Input;
using Application.AccessEvents.DTOs;
using Microsoft.Extensions.Logging;
using Mobile.AccessPoint.Services;
using Shared.DTOs.AccessEvents;

namespace Mobile.AccessPoint.ViewModels;

/// <summary>
/// ViewModel for NFC access control page following MVVM pattern.
/// AccessPoint siempre online - valida contra backend en tiempo real.
/// </summary>
public class AccessNfcViewModel : BaseViewModel
{
    private readonly INfcService _nfcService;
    private readonly IAccessEventApiService _accessEventApiService;
    private readonly ILogger<AccessNfcViewModel> _logger;
    
    // Hardcoded for testing - TODO: Get from authentication context
    // Usando userId 2 que fue creado por el seeder
    private readonly int _currentUserId = 2;

    // State properties
    private bool _isListening;
    private bool _nfcAvailable;
    private bool _nfcEnabled;
    private bool _isValidating;
    private bool _showResult;
    private bool _showControlPoint;
    private string _controlPointId = "1"; // Default to point 1
    
    // NFC Status
    private string _nfcAvailableText = "-";
    private Color _nfcAvailableColor = Colors.Gray;
    private string _nfcEnabledText = "-";
    private Color _nfcEnabledColor = Colors.Gray;
    
    // Scan Area
    private string _statusText = "Listo para Escanear";
    private string _instructionText = "Acerca tu tarjeta o dispositivo NFC al lector";
    private Color _scanFrameColor = Colors.LightBlue;
    
    // Control Point Info
    private string _controlPointText = string.Empty;
    
    // Result
    private string _resultIcon = string.Empty;
    private Color _resultIconColor = Colors.Gray;
    private string _resultTitle = string.Empty;
    private Color _resultTitleColor = Colors.Gray;
    private string _resultMessage = string.Empty;
    private Color _resultFrameBorderColor = Colors.Gray;
    private string _userText = string.Empty;
    private string _locationText = string.Empty;
    private string _dateTimeText = string.Empty;
    private string _tagIdText = string.Empty;

    public AccessNfcViewModel(
        INfcService nfcService,
        IAccessEventApiService accessEventApiService,
        ILogger<AccessNfcViewModel> logger)
    {
        _nfcService = nfcService;
        _accessEventApiService = accessEventApiService;
        _logger = logger;

        Title = "Control de Acceso NFC";

        // Subscribe to NFC events
        _nfcService.TagDetected += OnNfcTagDetected;

        // Initialize commands
        StartListeningCommand = new Command(async () => await StartListeningAsync(), () => !IsBusy && CanStartListening);
        StopListeningCommand = new Command(StopListening, () => IsListening);
        ViewHistoryCommand = new Command(async () => await ViewHistoryAsync());

        // Update NFC status
        UpdateNfcStatus();
    }

    #region Properties

    public bool IsListening
    {
        get => _isListening;
        set
        {
            if (SetProperty(ref _isListening, value))
            {
                OnPropertyChanged(nameof(StartButtonVisible));
                OnPropertyChanged(nameof(StopButtonVisible));
                ((Command)StartListeningCommand).ChangeCanExecute();
                ((Command)StopListeningCommand).ChangeCanExecute();
            }
        }
    }

    public bool NfcAvailable
    {
        get => _nfcAvailable;
        set => SetProperty(ref _nfcAvailable, value);
    }

    public bool NfcEnabled
    {
        get => _nfcEnabled;
        set => SetProperty(ref _nfcEnabled, value);
    }

    public bool IsValidating
    {
        get => _isValidating;
        set => SetProperty(ref _isValidating, value);
    }

    public bool ShowResult
    {
        get => _showResult;
        set => SetProperty(ref _showResult, value);
    }

    public bool ShowControlPoint
    {
        get => _showControlPoint;
        set => SetProperty(ref _showControlPoint, value);
    }

    public string ControlPointId
    {
        get => _controlPointId;
        set => SetProperty(ref _controlPointId, value);
    }

    public string NfcAvailableText
    {
        get => _nfcAvailableText;
        set => SetProperty(ref _nfcAvailableText, value);
    }

    public Color NfcAvailableColor
    {
        get => _nfcAvailableColor;
        set => SetProperty(ref _nfcAvailableColor, value);
    }

    public string NfcEnabledText
    {
        get => _nfcEnabledText;
        set => SetProperty(ref _nfcEnabledText, value);
    }

    public Color NfcEnabledColor
    {
        get => _nfcEnabledColor;
        set => SetProperty(ref _nfcEnabledColor, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string InstructionText
    {
        get => _instructionText;
        set => SetProperty(ref _instructionText, value);
    }

    public Color ScanFrameColor
    {
        get => _scanFrameColor;
        set => SetProperty(ref _scanFrameColor, value);
    }

    public string ControlPointText
    {
        get => _controlPointText;
        set => SetProperty(ref _controlPointText, value);
    }

    public string ResultIcon
    {
        get => _resultIcon;
        set => SetProperty(ref _resultIcon, value);
    }

    public Color ResultIconColor
    {
        get => _resultIconColor;
        set => SetProperty(ref _resultIconColor, value);
    }

    public string ResultTitle
    {
        get => _resultTitle;
        set => SetProperty(ref _resultTitle, value);
    }

    public Color ResultTitleColor
    {
        get => _resultTitleColor;
        set => SetProperty(ref _resultTitleColor, value);
    }

    public string ResultMessage
    {
        get => _resultMessage;
        set => SetProperty(ref _resultMessage, value);
    }

    public Color ResultFrameBorderColor
    {
        get => _resultFrameBorderColor;
        set => SetProperty(ref _resultFrameBorderColor, value);
    }

    public string UserText
    {
        get => _userText;
        set => SetProperty(ref _userText, value);
    }

    public string LocationText
    {
        get => _locationText;
        set => SetProperty(ref _locationText, value);
    }

    public string DateTimeText
    {
        get => _dateTimeText;
        set => SetProperty(ref _dateTimeText, value);
    }

    public string TagIdText
    {
        get => _tagIdText;
        set => SetProperty(ref _tagIdText, value);
    }

    public bool StartButtonVisible => !IsListening;
    public bool StopButtonVisible => IsListening;
    public bool CanStartListening => NfcAvailable && NfcEnabled;

    #endregion

    #region Commands

    public ICommand StartListeningCommand { get; }
    public ICommand StopListeningCommand { get; }
    public ICommand ViewHistoryCommand { get; }

    #endregion

    #region Methods

    public void UpdateNfcStatus()
    {
        NfcAvailable = _nfcService.IsAvailable;
        NfcEnabled = _nfcService.IsEnabled;

        NfcAvailableText = NfcAvailable ? "✓ Sí" : "✗ No";
        NfcAvailableColor = NfcAvailable ? Colors.Green : Colors.Red;

        NfcEnabledText = NfcEnabled ? "✓ Sí" : "✗ No";
        NfcEnabledColor = NfcEnabled ? Colors.Green : Colors.Red;

        if (!NfcAvailable)
        {
            InstructionText = "NFC no está disponible en este dispositivo";
        }
        else if (!NfcEnabled)
        {
            InstructionText = "Por favor, habilita NFC en la configuración del dispositivo";
        }
        else
        {
            InstructionText = "Acerca tu tarjeta NFC al dispositivo";
        }

        ((Command)StartListeningCommand).ChangeCanExecute();
    }

    private async Task StartListeningAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            await _nfcService.StartListeningAsync();

            IsListening = true;
            IsValidating = true;
            ShowResult = false;
            ShowControlPoint = false;

            StatusText = "Escuchando...";
            InstructionText = "Acerca tu tarjeta NFC al dispositivo";
            ScanFrameColor = Colors.LightGreen;

            _logger.LogInformation("NFC listening started");
        }
        catch (NotSupportedException)
        {
            await Shell.Current.DisplayAlert("Error", "NFC no está soportado en este dispositivo", "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting NFC listening");
            await Shell.Current.DisplayAlert("Error", $"Error al iniciar NFC: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StopListening()
    {
        _nfcService.StopListening();

        IsListening = false;
        IsValidating = false;

        StatusText = "Listo para Escanear";
        InstructionText = "Acerca tu tarjeta o dispositivo NFC al lector";
        ScanFrameColor = Colors.LightBlue;

        _logger.LogInformation("NFC listening stopped");
    }

    private async void OnNfcTagDetected(object? sender, NfcTagDetectedEventArgs e)
    {
        // Usar el ControlPointId ingresado por el usuario en lugar del que viene en el evento
        if (!int.TryParse(ControlPointId, out int controlPointIdToUse))
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert("Error", 
                    "El ID del Punto de Control debe ser un número válido.", 
                    "OK");
            });
            return;
        }
        
        _logger.LogInformation("NFC Tag detected in ViewModel: ControlPoint {ControlPointId}, Tag {TagId}, UserId {UserId}, CredentialId {CredentialId}",
            controlPointIdToUse, e.TagId, e.UserId, e.CredentialId);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                // Determinar si es credencial digital o tag tradicional
                // Si tiene CredentialId, es una credencial digital (ignorar UserId del payload)
                bool isDigitalCredential = e.CredentialId.HasValue;
                
                int? userIdToValidate = null;
                int? credentialIdToValidate = null;
                
                if (isDigitalCredential)
                {
                    // Para credenciales digitales: SOLO usar CredentialId
                    credentialIdToValidate = e.CredentialId;
                    _logger.LogInformation("🪪 Digital credential detected - CredentialId: {CredentialId}", 
                        e.CredentialId);
                }
                else
                {
                    // NO ACEPTAR TAGS SIN CREDENCIAL DIGITAL
                    _logger.LogWarning("❌ NFC tag detected without CredentialId - REJECTING");
                    StatusText = "❌ Tag NFC inválido";
                    InstructionText = "Este tag no tiene credencial digital válida";
                    ScanFrameColor = Colors.Red;
                    await Task.Delay(2000);
                    StatusText = "Listo para Escanear";
                    InstructionText = "Acerca tu credencial digital al lector";
                    ScanFrameColor = Colors.LightBlue;
                    return;
                }

                // Mostrar información del punto de control
                ShowControlPoint = true;
                ControlPointText = $"Punto de Control #{controlPointIdToUse}";

                // Mostrar validando
                StatusText = "Validando acceso con servidor...";
                ScanFrameColor = Colors.Orange;

                AccessValidationResult validationResult;
                DateTime eventDateTime = DateTime.UtcNow;

                try
                {
                    // SIEMPRE validar con backend - El punto de control NUNCA está offline
                    _logger.LogInformation("========================================");
                    _logger.LogInformation("🌐 VALIDATING WITH BACKEND");
                    _logger.LogInformation("UserId to send: {UserId}", userIdToValidate ?? -1);
                    _logger.LogInformation("CredentialId to send: {CredentialId}", credentialIdToValidate ?? -1);
                    _logger.LogInformation("ControlPointId: {ControlPointId}", controlPointIdToUse);
                    _logger.LogInformation("========================================");
                    
                    validationResult = await ValidateAccessAsync(userIdToValidate, credentialIdToValidate, controlPointIdToUse);
                    
                    _logger.LogInformation("✅ Validation result from backend: {Result}", validationResult.Result);

                    // Crear evento en backend - usar el userId del resultado de validación
                    var request = new CreateAccessEventRequest
                    {
                        UserId = validationResult.UserId,
                        ControlPointId = controlPointIdToUse,
                        EventDateTime = eventDateTime,
                        Result = validationResult.Result
                    };

                    var accessEvent = await _accessEventApiService.CreateAccessEventAsync(request);

                    _logger.LogInformation("✅ Access event created: EventId={EventId}, Result={Result}",
                        accessEvent.Id, accessEvent.Result);
                    
                    // 🆕 ENVIAR RESPUESTA VISUAL AL CELULAR CON CREDENCIAL
                    if (isDigitalCredential)
                    {
                        bool responseSent = false;
                        
                        if (validationResult.IsGranted)
                        {
                            _logger.LogInformation("📱 Sending ACCESS GRANTED notification to credential device...");
                            responseSent = await _nfcService.SendAccessGrantedAsync("✅ Acceso concedido");
                        }
                        else
                        {
                            _logger.LogInformation("📱 Sending ACCESS DENIED notification to credential device...");
                            responseSent = await _nfcService.SendAccessDeniedAsync(validationResult.Reason);
                        }
                        
                        if (responseSent)
                        {
                            _logger.LogInformation("✅ Visual response sent to credential device successfully");
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Could not send visual response to credential device (device may have moved away)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Si falla la conexión, el punto de control NO FUNCIONA
                    _logger.LogError(ex, "❌ BACKEND VALIDATION FAILED - Error: {Message}", ex.Message);
                    
                    // 🆕 Enviar respuesta de error al dispositivo credencial antes de mostrar error
                    if (isDigitalCredential)
                    {
                        _logger.LogInformation("📱 Sending ERROR notification to credential device...");
                        await _nfcService.SendAccessDeniedAsync($"❌ Error de servidor");
                    }
                    
                    await Shell.Current.DisplayAlert("Error de Conexión", 
                        $"No se pudo conectar al servidor:\n\n{ex.Message}\n\nEl punto de control requiere conexión al backend para funcionar.", 
                        "OK");
                    
                    return; // Salir sin mostrar resultado
                }

                // Mostrar resultado
                ShowAccessResult(validationResult, e.TagId, eventDateTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar NFC tag");
                await Shell.Current.DisplayAlert("Error", $"Error al procesar tag NFC: {ex.Message}", "OK");
            }
        });
    }

    private async Task<AccessValidationResult> ValidateAccessAsync(int? userId, int? credentialId, int controlPointId)
    {
        try
        {
            // Llamar al endpoint de validación del backend
            var response = await _accessEventApiService.ValidateAccessAsync(userId, credentialId, controlPointId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access");
            throw;
        }
    }

    private void ShowAccessResult(AccessValidationResult validation, string tagId, DateTime eventDateTime)
    {
        ShowResult = true;
        IsValidating = false;
        
        ScanFrameColor = validation.IsGranted ? Colors.LightGreen : Colors.LightCoral;
        StatusText = validation.IsGranted ? "Acceso Permitido" : "Acceso Denegado";

        if (validation.IsGranted)
        {
            ResultFrameBorderColor = Colors.Green;
            ResultIcon = "✓";
            ResultIconColor = Colors.Green;
            ResultTitle = "ACCESO PERMITIDO";
            ResultTitleColor = Colors.Green;
            ResultMessage = validation.Reason;
        }
        else
        {
            ResultFrameBorderColor = Colors.Red;
            ResultIcon = "✗";
            ResultIconColor = Colors.Red;
            ResultTitle = "ACCESO DENEGADO";
            ResultTitleColor = Colors.Red;
            ResultMessage = $"Motivo: {validation.Reason}";
        }

        UserText = validation.UserName;
        LocationText = validation.ControlPointName;
        DateTimeText = eventDateTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        TagIdText = tagId;

        // Auto-detener después de mostrar resultado
        Task.Delay(5000).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (IsListening)
                {
                    StopListening();
                }
            });
        });
    }

    private async Task ViewHistoryAsync()
    {
        // TODO: Navigate to history page
        await Shell.Current.DisplayAlert("Historial", "Funcionalidad de historial próximamente", "OK");
    }

    #endregion

    public void OnAppearing()
    {
        UpdateNfcStatus();
    }

    public void OnDisappearing()
    {
        if (IsListening)
        {
            StopListening();
        }
    }
}

