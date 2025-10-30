namespace Application.Roles.DTOs;

/// <summary>
/// Response DTO for role information.
/// </summary>
public class RoleResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
