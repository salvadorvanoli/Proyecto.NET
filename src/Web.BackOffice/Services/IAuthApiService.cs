using Application.Auth.DTOs;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for authentication through the API.
/// </summary>
public interface IAuthApiService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

