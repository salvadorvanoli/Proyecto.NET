using Shared.DTOs.Auth;
using System.Net.Http.Json;
using System.Text.Json;
using Mobile.Models;

namespace Mobile.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private LoginResponse? _currentUser;
    private SecureSession? _currentSession;
    private const string SessionStorageKey = "secure_session";
    private bool _sessionRestored = false;
    private readonly SemaphoreSlim _restoreLock = new SemaphoreSlim(1, 1);

    public bool IsAuthenticated => _currentUser != null && _currentSession != null && !_currentSession.IsTokenExpired();

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
            
            var response = await httpClient.PostAsJsonAsync("/api/auth/mobile-login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                _currentUser = await response.Content.ReadFromJsonAsync<LoginResponse>();
                
                // Guardar sesión de forma segura (sin contraseña)
                if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.Token))
                {
                    _currentSession = new SecureSession
                    {
                        UserId = _currentUser.UserId,
                        CredentialId = _currentUser.CredentialId,
                        Email = _currentUser.Email,
                        FullName = _currentUser.FullName,
                        TenantId = _currentUser.TenantId,
                        Roles = _currentUser.Roles,
                        Token = _currentUser.Token,
                        ExpiresAtUtc = _currentUser.ExpiresAtUtc ?? DateTime.UtcNow.AddHours(8)
                    };
                    
                    await SaveSessionAsync(_currentSession);
                    System.Diagnostics.Debug.WriteLine($"✅ Login exitoso: {_currentUser.Email}");
                    System.Diagnostics.Debug.WriteLine($"   Token expira: {_currentSession.ExpiresAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
                }
                
                return _currentUser;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                System.Diagnostics.Debug.WriteLine("❌ Login failed: Credenciales incorrectas");
                return null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ Login failed: {response.StatusCode}");
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error de red en login: {ex.Message}");
            throw new InvalidOperationException("No se pudo conectar con el servidor. Verifica tu conexión a internet.", ex);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error inesperado en login: {ex.Message}");
            throw new InvalidOperationException("Ocurrió un error inesperado. Por favor, intenta nuevamente.", ex);
        }
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _currentSession = null;
        _sessionRestored = false;
        
        // Limpiar sesión guardada
        try
        {
            SecureStorage.Remove(SessionStorageKey);
            System.Diagnostics.Debug.WriteLine("✅ Sesión cerrada y limpiada");
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
        
        // Verificar si el token expiró
        if (_currentSession != null && _currentSession.IsTokenExpired())
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Token expirado, cerrando sesión");
            await LogoutAsync();
            return null;
        }
        
        // Advertir si está por expirar
        if (_currentSession != null && _currentSession.IsTokenExpiringSoon())
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Token próximo a expirar");
            // TODO: Implementar refresh token aquí
        }
        
        return _currentUser;
    }
    
    private async Task SaveSessionAsync(SecureSession session)
    {
        try
        {
            var json = JsonSerializer.Serialize(session);
            await SecureStorage.SetAsync(SessionStorageKey, json);
            System.Diagnostics.Debug.WriteLine("✅ Sesión guardada en SecureStorage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error saving session: {ex.Message}");
        }
    }
    
    private async Task EnsureSessionRestoredAsync()
    {
        if (_sessionRestored) return;
        
        await _restoreLock.WaitAsync();
        try
        {
            if (_sessionRestored) return;
            
            var sessionJson = await SecureStorage.GetAsync(SessionStorageKey);
            if (!string.IsNullOrEmpty(sessionJson))
            {
                _currentSession = JsonSerializer.Deserialize<SecureSession>(sessionJson);
                
                if (_currentSession != null)
                {
                    // Verificar si el token expiró
                    if (_currentSession.IsTokenExpired())
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Sesión restaurada pero token expirado");
                        await LogoutAsync();
                    }
                    else
                    {
                        // Reconstruir LoginResponse desde SecureSession
                        _currentUser = new LoginResponse
                        {
                            UserId = _currentSession.UserId,
                            CredentialId = _currentSession.CredentialId,
                            Email = _currentSession.Email,
                            FullName = _currentSession.FullName,
                            TenantId = _currentSession.TenantId,
                            Roles = _currentSession.Roles,
                            Token = _currentSession.Token,
                            ExpiresAtUtc = _currentSession.ExpiresAtUtc
                        };
                        
                        System.Diagnostics.Debug.WriteLine($"✅ Sesión restaurada: {_currentUser.Email}");
                        System.Diagnostics.Debug.WriteLine($"   Token expira: {_currentSession.ExpiresAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
                    }
                }
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
