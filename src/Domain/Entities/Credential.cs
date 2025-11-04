using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a credential associated with a user for access control.
/// </summary>
public class Credential : BaseEntity
{
    /// <summary>
    /// Date and time when the credential was issued.
    /// </summary>
    public DateTime IssueDate { get; protected set; }

    /// <summary>
    /// Indicates whether the credential is active.
    /// </summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Foreign key to the user this credential belongs to.
    /// </summary>
    public int UserId { get; protected set; }

    // Navigation properties
    public virtual User User { get; protected set; } = null!;

    protected Credential() : base()
    {
    }

    public Credential(int tenantId, int userId, DateTime issueDate, bool isActive = true) : base(tenantId)
    {
        if (userId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de usuario"),
                nameof(userId));

        UserId = userId;
        IssueDate = issueDate;
        IsActive = isActive;
    }

    /// <summary>
    /// Creates a new credential with the current timestamp.
    /// </summary>
    public static Credential CreateNow(int tenantId, int userId)
    {
        return new Credential(tenantId, userId, DateTime.UtcNow, true);
    }

    /// <summary>
    /// Activates the credential.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Deactivates the credential.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Renews the credential by updating the issue date and activating it.
    /// </summary>
    public void Renew()
    {
        IssueDate = DateTime.UtcNow;
        IsActive = true;
        UpdateTimestamp();
    }
}

