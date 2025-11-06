using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a type of benefit that can be offered to users.
/// </summary>
public class BenefitType : BaseEntity
{
    /// <summary>
    /// Name of the benefit type.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Description of the benefit type.
    /// </summary>
    public string Description { get; protected set; }

    protected BenefitType() : base()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public BenefitType(int tenantId, string name, string description) : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"),
                nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Descripción"),
                nameof(description));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.BenefitTypeNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.BenefitTypeNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.BenefitTypeNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.BenefitTypeNameMaxLength),
                nameof(name));

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length < DomainConstants.StringLengths.DescriptionMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Descripción", DomainConstants.StringLengths.DescriptionMinLength),
                nameof(description));

        if (trimmedDescription.Length > DomainConstants.StringLengths.DescriptionMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Descripción", DomainConstants.StringLengths.DescriptionMaxLength),
                nameof(description));

        Name = trimmedName;
        Description = trimmedDescription;
    }

    /// <summary>
    /// Updates the benefit type information.
    /// </summary>
    public void UpdateInformation(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Nombre"),
                nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Descripción"),
                nameof(description));

        var trimmedName = name.Trim();
        if (trimmedName.Length < DomainConstants.StringLengths.BenefitTypeNameMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Nombre", DomainConstants.StringLengths.BenefitTypeNameMinLength),
                nameof(name));

        if (trimmedName.Length > DomainConstants.StringLengths.BenefitTypeNameMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Nombre", DomainConstants.StringLengths.BenefitTypeNameMaxLength),
                nameof(name));

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length < DomainConstants.StringLengths.DescriptionMinLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MinLengthRequired, "Descripción", DomainConstants.StringLengths.DescriptionMinLength),
                nameof(description));

        if (trimmedDescription.Length > DomainConstants.StringLengths.DescriptionMaxLength)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MaxLengthExceeded, "Descripción", DomainConstants.StringLengths.DescriptionMaxLength),
                nameof(description));

        Name = trimmedName;
        Description = trimmedDescription;
        UpdateTimestamp();
    }
}

