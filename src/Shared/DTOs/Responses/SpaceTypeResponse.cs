namespace Shared.DTOs.Responses;

/// <summary>
/// Response for space type information.
/// </summary>
public class SpaceTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public int SpaceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
