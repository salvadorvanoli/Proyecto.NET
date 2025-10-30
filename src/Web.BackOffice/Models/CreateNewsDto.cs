namespace Web.BackOffice.Models;

/// <summary>
/// DTO for creating a new news article.
/// </summary>
public class CreateNewsDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; } = DateTime.Now;
    public string? ImageUrl { get; set; }
}

