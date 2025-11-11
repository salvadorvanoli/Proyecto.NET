namespace Shared.DTOs.Auth;

/// <summary>
/// Response for successful login.
/// </summary>
public class LoginResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public List<string> Roles { get; set; } = new();
    // JWT token issued after successful authentication (optional)
    public string? Token { get; set; }

    // UTC expiration of the token
    public DateTime? ExpiresAtUtc { get; set; }
}
