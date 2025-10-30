namespace Application.News.DTOs;

/// <summary>
/// Request DTO for updating a news article.
/// </summary>
public class UpdateNewsRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public string? ImageUrl { get; set; }
}
