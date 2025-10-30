namespace Web.BackOffice.Models;

/// <summary>
/// DTO for updating a news article.
/// </summary>
public class UpdateNewsDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public string? ImageUrl { get; set; }
}
