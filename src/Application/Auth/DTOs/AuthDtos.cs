using Application.Users.DTOs;

namespace Application.Auth.DTOs;

/// <summary>
/// Request DTO for user login.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for successful login.
/// </summary>
public class LoginResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public List<string> Roles { get; set; } = new();
}

