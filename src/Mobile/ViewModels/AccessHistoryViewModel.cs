using Mobile.Models;
using Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Mobile.ViewModels;

public class AccessHistoryViewModel : BaseViewModel
{
    private readonly IAccessEventService _accessEventService;
    private readonly IDialogService _dialogService;
    private readonly SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);
    
    private bool _isLoading;
    private bool _isLoadingMore;
    private bool _hasMoreItems = true;
    private int _currentPage = 0;
    private const int PageSize = 20;

    public ObservableCollection<AccessEventDto> AccessEvents { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsLoadingMore
    {
        get => _isLoadingMore;
        set => SetProperty(ref _isLoadingMore, value);
    }

    public bool HasMoreItems
    {
        get => _hasMoreItems;
        set => SetProperty(ref _hasMoreItems, value);
    }

    public ICommand LoadEventsCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Initializes the ViewModel. Called when the page appears.
    /// </summary>
    public async Task InitializeAsync()
    {
        await RefreshEventsAsync();
    }

    public AccessHistoryViewModel(IAccessEventService accessEventService, IDialogService dialogService)
    {
        _accessEventService = accessEventService;
        _dialogService = dialogService;
        
        Title = "Historial de Accesos";
        
        LoadEventsCommand = new Command(async () => await LoadEventsAsync());
        LoadMoreCommand = new Command(async () => await LoadMoreEventsAsync());
        RefreshCommand = new Command(async () => await RefreshEventsAsync());

        System.Diagnostics.Debug.WriteLine("🔔 AccessHistoryViewModel constructor - Suscribiéndose a mensajes");

        // Suscribirse a notificaciones de nuevos eventos
        MessagingCenter.Subscribe<CredentialViewModel>(this, "AccessEventCreated", async (sender) =>
        {
            System.Diagnostics.Debug.WriteLine("📬 MENSAJE RECIBIDO: AccessEventCreated en AccessHistoryViewModel");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await RefreshEventsAsync();
                System.Diagnostics.Debug.WriteLine("✅ RefreshEventsAsync completado después de recibir AccessEventCreated");
            });
        });

        // Suscribirse a notificaciones de sincronización completada
        MessagingCenter.Subscribe<Services.SyncService>(this, "EventsSynced", async (sender) =>
        {
            System.Diagnostics.Debug.WriteLine("📬 MENSAJE RECIBIDO: EventsSynced en AccessHistoryViewModel");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await RefreshEventsAsync();
                System.Diagnostics.Debug.WriteLine("✅ RefreshEventsAsync completado después de recibir EventsSynced");
            });
        });
    }

    private async Task LoadEventsAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔄 LoadEventsAsync INICIADO");
        
        // Usar semáforo para evitar cargas concurrentes
        if (!await _loadSemaphore.WaitAsync(0))
        {
            System.Diagnostics.Debug.WriteLine("⚠️ LoadEventsAsync - Ya hay una carga en progreso, saliendo");
            return;
        }

        try
        {
            IsLoading = true;
            
            System.Diagnostics.Debug.WriteLine("🧹 Limpiando eventos actuales. Count antes: {0}", AccessEvents.Count);
            _currentPage = 0;
            AccessEvents.Clear();

            System.Diagnostics.Debug.WriteLine("🌐 Solicitando eventos al servicio (skip=0, take={0})", PageSize);
            var events = await _accessEventService.GetMyAccessEventsAsync(0, PageSize);
            System.Diagnostics.Debug.WriteLine("📦 Eventos recibidos del servicio: {0}", events.Count);
            
            foreach (var evt in events)
            {
                AccessEvents.Add(evt);
            }

            System.Diagnostics.Debug.WriteLine("✅ Eventos agregados a la colección. Count final: {0}", AccessEvents.Count);
            HasMoreItems = events.Count == PageSize;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading events: {ex.Message}");
            await _dialogService.ShowAlertAsync(
                "Error",
                "No se pudieron cargar los eventos de acceso");
        }
        finally
        {
            IsLoading = false;
            _loadSemaphore.Release();
            System.Diagnostics.Debug.WriteLine("✅ LoadEventsAsync COMPLETADO - Semáforo liberado");
        }
    }

    private async Task LoadMoreEventsAsync()
    {
        if (IsLoadingMore || !HasMoreItems)
            return;

        IsLoadingMore = true;

        try
        {
            _currentPage++;
            var skip = _currentPage * PageSize;

            var events = await _accessEventService.GetMyAccessEventsAsync(skip, PageSize);
            
            foreach (var evt in events)
            {
                AccessEvents.Add(evt);
            }

            HasMoreItems = events.Count == PageSize;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading more events: {ex.Message}");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    public async Task RefreshEventsAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔄 RefreshEventsAsync LLAMADO");
        await LoadEventsAsync();
        System.Diagnostics.Debug.WriteLine("✅ LoadEventsAsync completado desde RefreshEventsAsync");
    }
}
