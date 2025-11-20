using Shared.DTOs.Auth;

namespace Mobile.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<LoginResponse?> GetCurrentUserAsync();
    bool IsAuthenticated { get; }
}
