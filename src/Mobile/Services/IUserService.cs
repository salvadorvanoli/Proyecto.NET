using Mobile.Models;

namespace Mobile.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync();
    Task<bool> IsUserActiveAsync();
}
