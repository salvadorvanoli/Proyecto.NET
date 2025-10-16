using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a notification sent to a user.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Title of the notification.
    /// </summary>
    public string Title { get; protected set; }

    /// <summary>
    /// Content/message of the notification.
    /// </summary>
    public string Message { get; protected set; }

    /// <summary>
    /// Date and time when the notification was sent.
    /// </summary>
    public DateTime SentDateTime { get; protected set; }

    /// <summary>
    /// Indicates whether the notification has been read by the user.
    /// </summary>
    public bool IsRead { get; protected set; }

    /// <summary>
    /// Foreign key to the user who received the notification.
    /// </summary>
    public int UserId { get; protected set; }

    // Navigation properties
    public virtual User User { get; protected set; } = null!;

    protected Notification() : base()
    {
        Title = string.Empty;
        Message = string.Empty;
    }

    public Notification(int tenantId, string title, string message, int userId, DateTime sentDateTime)
        : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, nameof(title)), nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, nameof(message)), nameof(message));

        if (userId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "User ID"), nameof(userId));

        Title = title.Trim();
        Message = message.Trim();
        UserId = userId;
        SentDateTime = sentDateTime;
        IsRead = false;
    }

    /// <summary>
    /// Creates a notification with the current timestamp.
    /// </summary>
    public static Notification CreateNow(int tenantId, string title, string message, int userId)
    {
        return new Notification(tenantId, title, message, userId, DateTime.UtcNow);
    }

    /// <summary>
    /// Marks the notification as read.
    /// </summary>
    public void MarkAsRead()
    {
        IsRead = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Marks the notification as unread.
    /// </summary>
    public void MarkAsUnread()
    {
        IsRead = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the notification content.
    /// </summary>
    public void UpdateContent(string title, string message)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, nameof(title)), nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, nameof(message)), nameof(message));

        Title = title.Trim();
        Message = message.Trim();
        UpdateTimestamp();
    }
}

