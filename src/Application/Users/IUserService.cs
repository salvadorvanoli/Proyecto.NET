using Shared.DTOs.Users;

namespace Application.Users;

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
    /// Gets all users from the current tenant.
    /// </summary>
    Task<IEnumerable<UserResponse>> GetUsersByTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
}
