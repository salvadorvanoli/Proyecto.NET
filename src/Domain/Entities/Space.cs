using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a physical space that can be accessed and controlled.
/// </summary>
public class Space : BaseEntity
{
    /// <summary>
    /// Name of the space.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Foreign key to the space type.
    /// </summary>
    public int SpaceTypeId { get; protected set; }

    // Navigation properties
    public virtual SpaceType SpaceType { get; protected set; } = null!;
    public virtual ICollection<ControlPoint> ControlPoints { get; protected set; } = new List<ControlPoint>();

    protected Space() : base()
    {
        Name = string.Empty;
    }

    public Space(int tenantId, string name, int spaceTypeId) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"),
                nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.SpaceNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.SpaceNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.SpaceNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.SpaceNameMaxLength),
                nameof(name));

        if (spaceTypeId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de tipo de espacio"),
                nameof(spaceTypeId));

        Name = trimmedName;
        SpaceTypeId = spaceTypeId;
    }

    /// <summary>
    /// Updates the space information.
    /// </summary>
    public void UpdateInformation(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"),
                nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.SpaceNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.SpaceNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.SpaceNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.SpaceNameMaxLength),
                nameof(name));

        Name = trimmedName;
        UpdateTimestamp();
    }

    /// <summary>
    /// Changes the space type.
    /// </summary>
    public void ChangeSpaceType(int spaceTypeId)
    {
        if (spaceTypeId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de tipo de espacio"),
                nameof(spaceTypeId));

        SpaceTypeId = spaceTypeId;
        UpdateTimestamp();
    }
}

