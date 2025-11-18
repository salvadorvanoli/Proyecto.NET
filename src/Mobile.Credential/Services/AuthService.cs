using Mobile.Credential.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mobile.Credential.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private LoginResponse? _currentUser;
    private const string UserStorageKey = "current_user_session";
    private bool _sessionRestored = false;
    private readonly SemaphoreSlim _restoreLock = new SemaphoreSlim(1, 1);

    public bool IsAuthenticated => _currentUser != null;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("AuthClient");
            
            // Use email as username
            var loginRequest = new { Email = username, Password = password };
            
            var response = await httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                _currentUser = await response.Content.ReadFromJsonAsync<LoginResponse>();
                
                // Guardar sesión de forma segura
                if (_currentUser != null)
                {
                    await SaveSessionAsync(_currentUser);
                }
                
                return _currentUser;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        
        // Limpiar sesión guardada
        try
        {
            SecureStorage.Remove(UserStorageKey);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing session: {ex.Message}");
        }
        
        await Task.CompletedTask;
    }

    public async Task<LoginResponse?> GetCurrentUserAsync()
    {
        // Lazy load: restaurar sesión solo la primera vez que se solicita
        await EnsureSessionRestoredAsync();
        return _currentUser;
    }
    
    private async Task SaveSessionAsync(LoginResponse user)
    {
        try
        {
            var json = JsonSerializer.Serialize(user);
            await SecureStorage.SetAsync(UserStorageKey, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving session: {ex.Message}");
        }
    }
    
    private async Task EnsureSessionRestoredAsync()
    {
        if (_sessionRestored) return;
        
        await _restoreLock.WaitAsync();
        try
        {
            if (_sessionRestored) return;
            
            var sessionJson = await SecureStorage.GetAsync(UserStorageKey);
            if (!string.IsNullOrEmpty(sessionJson))
            {
                _currentUser = JsonSerializer.Deserialize<LoginResponse>(sessionJson);
                System.Diagnostics.Debug.WriteLine($"✅ Sesión restaurada: {_currentUser?.Email}");
            }
            
            _sessionRestored = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error restaurando sesión: {ex.Message}");
            _sessionRestored = true; // Marcar como intentado aunque falle
        }
        finally
        {
            _restoreLock.Release();
        }
    }
}

