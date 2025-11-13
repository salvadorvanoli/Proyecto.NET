using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a type of space that can be used to categorize spaces.
/// </summary>
public class SpaceType : BaseEntity
{
    /// <summary>
    /// Name of the space type.
    /// </summary>
    public string Name { get; protected set; }

    protected SpaceType() : base()
    {
        Name = string.Empty;
    }

    public SpaceType(int tenantId, string name) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"), nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.SpaceTypeNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.SpaceTypeNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.SpaceTypeNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.SpaceTypeNameMaxLength),
                nameof(name));

        Name = trimmedName;
    }

    /// <summary>
    /// Updates the space type name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"), nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.SpaceTypeNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.SpaceTypeNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.SpaceTypeNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.SpaceTypeNameMaxLength),
                nameof(name));

        Name = trimmedName;
        UpdateTimestamp();
    }
}

