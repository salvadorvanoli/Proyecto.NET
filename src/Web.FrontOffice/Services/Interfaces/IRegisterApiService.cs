using Shared.DTOs.Users;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Interface for user registration API service.
/// </summary>
public interface IRegisterApiService
{
    Task<UserResponse?> RegisterAsync(CreateUserRequest request);
}
