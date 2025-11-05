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
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Benefit type name"),
                nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Description"),
                nameof(description));

        Name = name.Trim();
        Description = description.Trim();
    }

    /// <summary>
    /// Updates the benefit type information.
    /// </summary>
    public void UpdateInformation(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Benefit type name"),
                nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.CannotBeNullOrEmpty, "Description"),
                nameof(description));

        Name = name.Trim();
        Description = description.Trim();
        UpdateTimestamp();
    }
}

