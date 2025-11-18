namespace Mobile.AccessPoint.Models;

public class LoginResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? Token { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}

