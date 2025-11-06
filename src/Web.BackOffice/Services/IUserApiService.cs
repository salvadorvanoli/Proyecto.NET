using Shared.DTOs.Users;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing users through the API.
/// </summary>
public interface IUserApiService
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest createUserDto);
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest updateUserDto);
    Task<bool> DeleteUserAsync(int id);
}
