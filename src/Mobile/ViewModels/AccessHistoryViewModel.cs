using Mobile.Models;
using Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Mobile.ViewModels;

public class AccessHistoryViewModel : BaseViewModel
{
    private readonly IAccessEventService _accessEventService;
    
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

    public AccessHistoryViewModel(IAccessEventService accessEventService)
    {
        _accessEventService = accessEventService;
        
        Title = "Historial de Accesos";
        
        LoadEventsCommand = new Command(async () => await LoadEventsAsync());
        LoadMoreCommand = new Command(async () => await LoadMoreEventsAsync());
        RefreshCommand = new Command(async () => await RefreshEventsAsync());
    }

    public async Task InitializeAsync()
    {
        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            _currentPage = 0;
            AccessEvents.Clear();

            var events = await _accessEventService.GetMyAccessEventsAsync(0, PageSize);
            
            foreach (var evt in events)
            {
                AccessEvents.Add(evt);
            }

            HasMoreItems = events.Count == PageSize;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading events: {ex.Message}");
            await Shell.Current.DisplayAlert(
                "Error",
                "No se pudieron cargar los eventos de acceso",
                "OK");
        }
        finally
        {
            IsLoading = false;
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

    private async Task RefreshEventsAsync()
    {
        await LoadEventsAsync();
    }
}
