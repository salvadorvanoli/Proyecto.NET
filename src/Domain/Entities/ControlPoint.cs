using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a control point within a space for access management.
/// </summary>
public class ControlPoint : BaseEntity
{
    /// <summary>
    /// Name of the control point.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Foreign key to the space this control point belongs to.
    /// </summary>
    public int SpaceId { get; protected set; }

    // Navigation properties
    public virtual Space Space { get; protected set; } = null!;
    public virtual ICollection<AccessRule> AccessRules { get; protected set; } = new List<AccessRule>();
    public virtual ICollection<AccessEvent> AccessEvents { get; protected set; } = new List<AccessEvent>();

    protected ControlPoint() : base()
    {
        Name = string.Empty;
    }

    public ControlPoint(int tenantId, string name, int spaceId) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"),
                nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.ControlPointNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.ControlPointNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.ControlPointNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.ControlPointNameMaxLength),
                nameof(name));

        if (spaceId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de espacio"),
                nameof(spaceId));

        Name = trimmedName;
        SpaceId = spaceId;
    }

    /// <summary>
    /// Updates the control point name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"),
                nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.ControlPointNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.ControlPointNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.ControlPointNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.ControlPointNameMaxLength),
                nameof(name));

        Name = trimmedName;
        UpdateTimestamp();
    }

    /// <summary>
    /// Moves the control point to a different space.
    /// </summary>
    public void MoveToSpace(int spaceId)
    {
        if (spaceId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de espacio"),
                nameof(spaceId));

        SpaceId = spaceId;
        UpdateTimestamp();
    }
}

