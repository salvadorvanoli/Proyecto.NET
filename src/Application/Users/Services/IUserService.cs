using Application.Users.DTOs;

namespace Application.Users.Services;

/// <summary>
/// Service interface for user operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user in the current tenant context.
    /// </summary>
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<UserResponse?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    Task<UserResponse?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users from all tenants (admin operation).
    /// </summary>
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users from the current tenant.
    /// </summary>
    Task<IEnumerable<UserResponse>> GetUsersByTenantAsync(CancellationToken cancellationToken = default);
}
