using Shared.DTOs.Users;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Service for managing users through the API.
/// </summary>
public interface IUserApiService
{
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse?> GetCurrentUserAsync();
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest updateUserRequest);
}
