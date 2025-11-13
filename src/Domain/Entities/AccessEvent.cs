using Domain.Constants;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Represents an access event that occurred at a control point.
/// </summary>
public class AccessEvent : BaseEntity
{
    /// <summary>
    /// Date and time when the access event occurred.
    /// </summary>
    public DateTime EventDateTime { get; protected set; }

    /// <summary>
    /// Result of the access attempt.
    /// </summary>
    public AccessResult Result { get; protected set; }

    /// <summary>
    /// Foreign key to the control point where the event occurred.
    /// </summary>
    public int ControlPointId { get; protected set; }

    /// <summary>
    /// Foreign key to the user who attempted access.
    /// </summary>
    public int UserId { get; protected set; }

    // Navigation properties
    public virtual ControlPoint ControlPoint { get; protected set; } = null!;
    public virtual User User { get; protected set; } = null!;

    protected AccessEvent() : base()
    {
    }

    public AccessEvent(int tenantId, DateTime eventDateTime, AccessResult result,
                      int controlPointId, int userId) : base(tenantId)
    {
        if (controlPointId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID"),
                nameof(controlPointId));

        if (userId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de usuario"),
                nameof(userId));

        EventDateTime = eventDateTime;
        Result = result;
        ControlPointId = controlPointId;
        UserId = userId;
    }

    /// <summary>
    /// Creates an access event with the current timestamp.
    /// </summary>
    public static AccessEvent CreateNow(int tenantId, AccessResult result, int controlPointId, int userId)
    {
        return new AccessEvent(tenantId, DateTime.UtcNow, result, controlPointId, userId);
    }

    /// <summary>
    /// Checks if this was a successful access event.
    /// </summary>
    public bool WasGranted => Result == AccessResult.Granted;

    /// <summary>
    /// Checks if this was a denied access event.
    /// </summary>
    public bool WasDenied => Result == AccessResult.Denied;
}

