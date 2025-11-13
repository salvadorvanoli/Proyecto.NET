namespace Shared.DTOs.News;

/// <summary>
/// Response DTO for news information.
/// </summary>
public class NewsResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public string? ImageUrl { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
