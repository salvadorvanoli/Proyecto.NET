using Mobile.Models;
using Mobile.Services;
using System.Text.Json;
using System.Windows.Input;

namespace Mobile.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    
    private UserProfileDto? _profile;
    private bool _isLoadingProfile;
    private bool _isOfflineMode;

    public UserProfileDto? Profile
    {
        get => _profile;
        set => SetProperty(ref _profile, value);
    }

    public bool IsLoadingProfile
    {
        get => _isLoadingProfile;
        set => SetProperty(ref _isLoadingProfile, value);
    }

    public bool IsOfflineMode
    {
        get => _isOfflineMode;
        set => SetProperty(ref _isOfflineMode, value);
    }

    public ICommand LoadProfileCommand { get; }
    public ICommand LogoutCommand { get; }

    public ProfileViewModel(IUserService userService, IAuthService authService, INavigationService navigationService, IDialogService dialogService)
    {
        _userService = userService;
        _authService = authService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        
        Title = "Mi Perfil";
        
        LoadProfileCommand = new Command(async () => await LoadProfileAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    public async Task InitializeAsync()
    {
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        if (IsLoadingProfile)
            return;

        IsLoadingProfile = true;
        IsOfflineMode = false;

        try
        {
            System.Diagnostics.Debug.WriteLine("ProfileViewModel: Starting LoadProfileAsync");
            
            // Primero intentar cargar desde caché para mostrar algo rápido
            var cachedProfile = await LoadProfileFromStorageAsync();
            if (cachedProfile != null)
            {
                Profile = cachedProfile;
                IsOfflineMode = true;
                System.Diagnostics.Debug.WriteLine("ProfileViewModel: Loaded from cache");
            }
            
            // Luego intentar actualizar desde el servidor si hay conexión
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var profileFromServer = await _userService.GetProfileAsync();
                    
                    if (profileFromServer != null)
                    {
                        Profile = profileFromServer;
                        await SaveProfileToStorageAsync(profileFromServer);
                        IsOfflineMode = false;
                        System.Diagnostics.Debug.WriteLine("ProfileViewModel: Loaded from server");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ProfileViewModel: Server fetch failed: {ex.Message}");
                    // Mantener el perfil del caché si la actualización falla
                }
            }

            // Si aún no hay perfil, crear uno básico desde la sesión actual
            if (Profile == null)
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    var nameParts = (currentUser.FullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    Profile = new UserProfileDto
                    {
                        Id = currentUser.UserId,
                        Email = currentUser.Email ?? "",
                        FirstName = nameParts.FirstOrDefault() ?? "Usuario",
                        LastName = string.Join(" ", nameParts.Skip(1)),
                        IsActive = true
                    };
                    IsOfflineMode = true;
                    System.Diagnostics.Debug.WriteLine("ProfileViewModel: Created from current user");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProfileViewModel: Error in LoadProfileAsync: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            IsLoadingProfile = false;
        }
    }

    private async Task SaveProfileToStorageAsync(UserProfileDto profile)
    {
        try
        {
            var json = JsonSerializer.Serialize(profile);
            await SecureStorage.Default.SetAsync("user_profile_cache", json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving profile to storage: {ex.Message}");
        }
    }

    private async Task<UserProfileDto?> LoadProfileFromStorageAsync()
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync("user_profile_cache");
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<UserProfileDto>(json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile from storage: {ex.Message}");
        }
        
        return null;
    }

    private async Task LogoutAsync()
    {
        var confirm = await _dialogService.ShowConfirmAsync(
            "Cerrar Sesión",
            "¿Estás seguro que deseas cerrar sesión?",
            "Sí",
            "No");

        if (confirm)
        {
            await _authService.LogoutAsync();
            
            // Limpiar caché de perfil
            SecureStorage.Remove("user_profile_cache");
            
            // Navegar a login
            await _navigationService.NavigateToAsync("//LoginPage");
        }
    }
}
