using System.Windows.Input;
using Application.AccessEvents.DTOs;
using Microsoft.Extensions.Logging;
using Mobile.Services;
using Mobile.Data;
using Shared.DTOs.AccessEvents;

namespace Mobile.ViewModels;

/// <summary>
/// ViewModel for NFC access control page following MVVM pattern.
/// </summary>
public class AccessNfcViewModel : BaseViewModel
{
    private readonly INfcService _nfcService;
    private readonly IAccessEventApiService _accessEventApiService;
    private readonly ILocalDatabase _localDatabase;
    private readonly ISyncService _syncService;
    private readonly IMobileAccessRuleService _accessRuleService;
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
    private bool _isOnline;
    private int _pendingSyncCount;
    
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
        ILocalDatabase localDatabase,
        ISyncService syncService,
        IMobileAccessRuleService accessRuleService,
        ILogger<AccessNfcViewModel> logger)
    {
        _nfcService = nfcService;
        _accessEventApiService = accessEventApiService;
        _localDatabase = localDatabase;
        _syncService = syncService;
        _accessRuleService = accessRuleService;
        _logger = logger;

        Title = "Control de Acceso NFC";

        // Subscribe to NFC events
        _nfcService.TagDetected += OnNfcTagDetected;

        // Subscribe to sync events
        _syncService.ConnectivityChanged += OnConnectivityChanged;
        _syncService.SyncStatusChanged += OnSyncStatusChanged;

        // Initialize commands
        StartListeningCommand = new Command(async () => await StartListeningAsync(), () => !IsBusy && CanStartListening);
        StopListeningCommand = new Command(StopListening, () => IsListening);
        ViewHistoryCommand = new Command(async () => await ViewHistoryAsync());
        SyncNowCommand = new Command(async () => await SyncNowAsync(), () => IsOnline && PendingSyncCount > 0);
        SyncRulesCommand = new Command(async () => await SyncRulesAsync(), () => IsOnline && !IsBusy);

        // Update NFC status
        UpdateNfcStatus();
        
        // Update connectivity status
        UpdateConnectivityStatus();
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

    public bool IsOnline
    {
        get => _isOnline;
        set
        {
            if (SetProperty(ref _isOnline, value))
            {
                OnPropertyChanged(nameof(ConnectivityStatusText));
                OnPropertyChanged(nameof(ConnectivityStatusColor));
                ((Command)SyncNowCommand).ChangeCanExecute();
            }
        }
    }

    public int PendingSyncCount
    {
        get => _pendingSyncCount;
        set
        {
            if (SetProperty(ref _pendingSyncCount, value))
            {
                OnPropertyChanged(nameof(ShowSyncBadge));
                OnPropertyChanged(nameof(SyncBadgeText));
                ((Command)SyncNowCommand).ChangeCanExecute();
            }
        }
    }

    public string ConnectivityStatusText => IsOnline ? "● Online" : "● Offline";
    public Color ConnectivityStatusColor => IsOnline ? Colors.Green : Colors.Orange;
    public bool ShowSyncBadge => PendingSyncCount > 0;
    public string SyncBadgeText => $"{PendingSyncCount} pendiente{(PendingSyncCount != 1 ? "s" : "")}";

    #endregion

    #region Commands

    public ICommand StartListeningCommand { get; }
    public ICommand StopListeningCommand { get; }
    public ICommand ViewHistoryCommand { get; }
    public ICommand SyncNowCommand { get; }
    public ICommand SyncRulesCommand { get; }

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

    private async void UpdateConnectivityStatus()
    {
        IsOnline = _syncService.IsConnected;
        PendingSyncCount = await _syncService.GetPendingSyncCountAsync();
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
        _logger.LogInformation("NFC Tag detected in ViewModel: ControlPoint {ControlPointId}, Tag {TagId}, UserId {UserId}, CredentialId {CredentialId}",
            e.ControlPointId, e.TagId, e.UserId, e.CredentialId);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                // Determinar el userId a usar
                // Si viene del HCE (credencial digital), usar ese userId
                // Si no, usar el userId hardcodeado del punto de control
                int userIdToValidate = e.UserId ?? _currentUserId;
                
                bool isDigitalCredential = e.UserId.HasValue && e.CredentialId.HasValue;
                
                if (isDigitalCredential)
                {
                    _logger.LogInformation("🪪 Digital credential detected - UserId: {UserId}, CredentialId: {CredentialId}", 
                        e.UserId, e.CredentialId);
                }
                else
                {
                    _logger.LogInformation("🏷️ Traditional NFC tag detected - Using control point user: {UserId}", 
                        userIdToValidate);
                }

                // Mostrar información del punto de control
                ShowControlPoint = true;
                ControlPointText = $"{e.ControlPointName} (ID: {e.ControlPointId})";

                // Mostrar validando
                StatusText = IsOnline ? "Validando acceso..." : "Validando (Offline)...";
                ScanFrameColor = Colors.Orange;

                AccessValidationResult validationResult;
                LocalAccessEvent localEvent;
                DateTime eventDateTime = DateTime.UtcNow;

                if (IsOnline)
                {
                    try
                    {
                        // Validar con backend
                        _logger.LogInformation("🌐 ONLINE MODE - Attempting to validate with backend");
                        _logger.LogInformation("Backend URL configured: http://192.168.1.23:5000");
                        _logger.LogInformation("UserId: {UserId}, ControlPointId: {ControlPointId}", userIdToValidate, e.ControlPointId);
                        
                        validationResult = await ValidateAccessAsync(userIdToValidate, e.ControlPointId);
                        
                        _logger.LogInformation("✅ Validation successful from backend: {Result}", validationResult.Result);

                        // Crear evento en backend
                        var request = new CreateAccessEventRequest
                        {
                            UserId = userIdToValidate,
                            ControlPointId = e.ControlPointId,
                            EventDateTime = eventDateTime,
                            Result = validationResult.Result
                        };

                        var accessEvent = await _accessEventApiService.CreateAccessEventAsync(request);

                        _logger.LogInformation("✅ Access event created online: RemoteId={EventId}, Result={Result}. NOT saving to local DB (already in backend)",
                            accessEvent.Id, accessEvent.Result);
                        
                        // NO guardar localmente porque ya está en el backend
                        // Esto evita duplicados y eventos "pendientes" innecesarios
                    }
                    catch (Exception ex)
                    {
                        // Si falla online, caer en modo offline
                        _logger.LogError(ex, "❌ ONLINE VALIDATION FAILED - Error: {Message}", ex.Message);
                        _logger.LogError("Exception Type: {Type}", ex.GetType().Name);
                        if (ex.InnerException != null)
                        {
                            _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
                        }
                        _logger.LogWarning("⚠️ Falling back to OFFLINE mode");
                        
                        // MOSTRAR EL ERROR AL USUARIO
                        await Shell.Current.DisplayAlert("Error de Conexión", 
                            $"No se pudo conectar al backend:\n\n{ex.Message}\n\nUsando modo offline.", 
                            "OK");
                        
                        validationResult = await ValidateAccessOfflineAsync(userIdToValidate, e.ControlPointId);
                        localEvent = await SaveEventOfflineAsync(validationResult, e, eventDateTime, userIdToValidate);
                    }
                }
                else
                {
                    // Modo offline
                    _logger.LogInformation("Processing in offline mode");
                    validationResult = await ValidateAccessOfflineAsync(userIdToValidate, e.ControlPointId);
                    localEvent = await SaveEventOfflineAsync(validationResult, e, eventDateTime, userIdToValidate);
                }

                // Mostrar resultado
                ShowAccessResult(validationResult, e.TagId, eventDateTime);

                // Actualizar contador de sync pendientes
                UpdateConnectivityStatus();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar NFC tag");
                await Shell.Current.DisplayAlert("Error", $"Error al procesar tag NFC: {ex.Message}", "OK");
            }
        });
    }

    private async Task<AccessValidationResult> ValidateAccessAsync(int userId, int controlPointId)
    {
        try
        {
            // Llamar al endpoint de validación del backend
            var response = await _accessEventApiService.ValidateAccessAsync(userId, controlPointId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access");
            throw;
        }
    }

    private async Task<AccessValidationResult> ValidateAccessOfflineAsync(int userId, int controlPointId)
    {
        // Use the access rule service for proper offline validation
        return await _accessRuleService.ValidateAccessOfflineAsync(userId, controlPointId, DateTime.UtcNow);
    }

    private async Task<LocalAccessEvent> SaveEventOfflineAsync(
        AccessValidationResult validation, 
        NfcTagDetectedEventArgs tagInfo,
        DateTime eventDateTime,
        int userId)
    {
        var localEvent = new LocalAccessEvent
        {
            UserId = userId,
            ControlPointId = tagInfo.ControlPointId,
            EventDateTime = eventDateTime,
            Result = validation.Result,
            Reason = validation.Reason,
            UserName = validation.UserName,
            ControlPointName = tagInfo.ControlPointName,
            TagId = tagInfo.TagId,
            IsSynced = false
        };

        await _localDatabase.SaveAccessEventAsync(localEvent);
        
        _logger.LogInformation("Saved access event offline: LocalId {Id}, Result: {Result}",
            localEvent.Id, localEvent.Result);

        return localEvent;
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

    private async Task SyncNowAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            _logger.LogInformation("Manual sync initiated");

            var syncedCount = await _syncService.SyncPendingEventsAsync();

            await Shell.Current.DisplayAlert(
                "Sincronización Completada",
                $"Se sincronizaron {syncedCount} evento(s) con el servidor.",
                "OK");

            UpdateConnectivityStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual sync");
            await Shell.Current.DisplayAlert("Error", $"Error al sincronizar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SyncRulesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            _logger.LogInformation("Manual rules sync initiated");

            var syncedCount = await _accessRuleService.SyncAccessRulesAsync();

            var cachedCount = await _localDatabase.GetCachedRulesCountAsync();

            await Shell.Current.DisplayAlert(
                "Reglas Sincronizadas",
                $"Se descargaron {syncedCount} reglas de acceso.\\nTotal en caché: {cachedCount}\\n\\nAhora puedes validar accesos en modo offline.",
                "OK");

            _logger.LogInformation("Rules synced successfully: {Count}", syncedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual rules sync");
            await Shell.Current.DisplayAlert("Error", $"Error al sincronizar reglas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnConnectivityChanged(object? sender, Services.ConnectivityChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsOnline = e.IsConnected;
            
            _logger.LogInformation("Connectivity changed in ViewModel: {Status}", 
                e.IsConnected ? "Online" : "Offline");
        });
    }

    private void OnSyncStatusChanged(object? sender, SyncStatusChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (e.IsCompleted)
            {
                _logger.LogInformation("Sync completed: {Success} successful, {Failed} failed",
                    e.SuccessfulSync, e.FailedSync);
                
                UpdateConnectivityStatus();
            }
        });
    }

    #endregion

    public void OnAppearing()
    {
        UpdateNfcStatus();
        UpdateConnectivityStatus();
    }

    public void OnDisappearing()
    {
        if (IsListening)
        {
            StopListening();
        }
    }
}
