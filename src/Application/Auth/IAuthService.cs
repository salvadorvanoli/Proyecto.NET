using Shared.DTOs.Auth;

namespace Application.Auth;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a user has a specific role.
    /// </summary>
    Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);
}

