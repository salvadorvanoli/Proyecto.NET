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

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length < DomainConstants.StringLengths.TitleMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Título", DomainConstants.StringLengths.TitleMinLength),
                nameof(title));

        if (trimmedTitle.Length > DomainConstants.StringLengths.TitleMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Título", DomainConstants.StringLengths.TitleMaxLength),
                nameof(title));

        var trimmedContent = content.Trim();
        if (trimmedContent.Length < DomainConstants.StringLengths.ContentMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Contenido", DomainConstants.StringLengths.ContentMinLength),
                nameof(content));

        if (trimmedContent.Length > DomainConstants.StringLengths.ContentMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Contenido", DomainConstants.StringLengths.ContentMaxLength),
                nameof(content));

        Title = trimmedTitle;
        Content = trimmedContent;
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

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length < DomainConstants.StringLengths.TitleMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Título", DomainConstants.StringLengths.TitleMinLength),
                nameof(title));

        if (trimmedTitle.Length > DomainConstants.StringLengths.TitleMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Título", DomainConstants.StringLengths.TitleMaxLength),
                nameof(title));

        var trimmedContent = content.Trim();
        if (trimmedContent.Length < DomainConstants.StringLengths.ContentMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Contenido", DomainConstants.StringLengths.ContentMinLength),
                nameof(content));

        if (trimmedContent.Length > DomainConstants.StringLengths.ContentMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Contenido", DomainConstants.StringLengths.ContentMaxLength),
                nameof(content));

        Title = trimmedTitle;
        Content = trimmedContent;
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
