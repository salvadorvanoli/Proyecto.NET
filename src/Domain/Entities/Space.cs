using Domain.Constants;
using Domain.DataTypes;

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
    /// Location of the space.
    /// </summary>
    public Location Location { get; protected set; }

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

    public Space(int tenantId, string name, Location location, int spaceTypeId) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrWhiteSpace, "Space name"),
                nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.NameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Space name", DomainConstants.StringLengths.NameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.NameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Space name", DomainConstants.StringLengths.NameMaxLength),
                nameof(name));

        if (spaceTypeId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Space type ID"),
                nameof(spaceTypeId));

        Name = trimmedName;
        Location = location;
        SpaceTypeId = spaceTypeId;
    }

    /// <summary>
    /// Updates the space information.
    /// </summary>
    public void UpdateInformation(string name, Location location)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrWhiteSpace, "Space name"),
                nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.NameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Space name", DomainConstants.StringLengths.NameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.NameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Space name", DomainConstants.StringLengths.NameMaxLength),
                nameof(name));

        Name = trimmedName;
        Location = location;
        UpdateTimestamp();
    }

    /// <summary>
    /// Changes the space type.
    /// </summary>
    public void ChangeSpaceType(int spaceTypeId)
    {
        if (spaceTypeId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Space type ID"),
                nameof(spaceTypeId));

        SpaceTypeId = spaceTypeId;
        UpdateTimestamp();
    }
}

