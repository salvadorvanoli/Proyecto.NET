using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a news article in the system.
/// </summary>
public class News : BaseEntity
{
    /// <summary>
    /// Title of the news article.
    /// </summary>
    public string Title { get; protected set; }

    /// <summary>
    /// Content of the news article.
    /// </summary>
    public string Content { get; protected set; }

    /// <summary>
    /// Date when the news was published.
    /// </summary>
    public DateTime PublishDate { get; protected set; }

    /// <summary>
    /// URL or path to the news article image.
    /// </summary>
    public string? ImageUrl { get; protected set; }

    protected News() : base()
    {
        Title = string.Empty;
        Content = string.Empty;
    }

    public News(int tenantId, string title, string content, DateTime publishDate, string? imageUrl = null) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Título"),
                nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Contenido"),
                nameof(content));

        Title = title.Trim();
        Content = content.Trim();
        PublishDate = publishDate;
        ImageUrl = imageUrl?.Trim();
    }

    /// <summary>
    /// Updates the news article content.
    /// </summary>
    public void UpdateContent(string title, string content, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Título"),
                nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Contenido"),
                nameof(content));

        Title = title.Trim();
        Content = content.Trim();
        ImageUrl = imageUrl?.Trim();
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the publish date.
    /// </summary>
    public void UpdatePublishDate(DateTime publishDate)
    {
        PublishDate = publishDate;
        UpdateTimestamp();
    }
}
