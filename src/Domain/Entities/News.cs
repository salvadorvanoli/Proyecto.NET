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

    protected News() : base()
    {
        Title = string.Empty;
        Content = string.Empty;
    }

    public News(int tenantId, string title, string content, DateTime publishDate) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Title"),
                nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Content"),
                nameof(content));

        Title = title.Trim();
        Content = content.Trim();
        PublishDate = publishDate;
    }

    /// <summary>
    /// Updates the news article content.
    /// </summary>
    public void UpdateContent(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Title"),
                nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Content"),
                nameof(content));

        Title = title.Trim();
        Content = content.Trim();
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

