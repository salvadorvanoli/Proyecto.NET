using Shared.DTOs.Auth;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Service for authentication through the API.
/// </summary>
public interface IAuthApiService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
