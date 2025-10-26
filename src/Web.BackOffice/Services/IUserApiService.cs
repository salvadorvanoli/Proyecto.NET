using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing users through the API.
/// </summary>
public interface IUserApiService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(int id);
}
