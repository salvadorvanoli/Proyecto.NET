namespace Web.BackOffice.Models;

/// <summary>
/// DTO for space type response.
/// </summary>
public class SpaceTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public int SpaceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
