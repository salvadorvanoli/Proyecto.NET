using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a usage event of a consumption.
/// </summary>
public class Usage : BaseEntity
{
    /// <summary>
    /// Quantity of the benefit being used.
    /// </summary>
    public int Quantity { get; protected set; }

    /// <summary>
    /// Foreign key to the benefit being used.
    /// </summary>
    public int BenefitId { get; protected set; }

    /// <summary>
    /// Foreign key to the user who made the usage.
    /// </summary>
    public int UserId { get; protected set; }

    // Navigation properties
    public virtual Benefit Benefit { get; protected set; } = null!;
    public virtual ICollection<Consumption> Consumptions { get; protected set; } = new List<Consumption>();

    protected Usage() : base()
    {
    }

    public Usage(int tenantId, int benefitId, int userId, int quantity) : base(tenantId)
    {
        if (benefitId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de beneficio"), nameof(benefitId));

        if (userId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de usuario"), nameof(userId));

        if (quantity < DomainConstants.NumericValidation.MinQuantity)
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cantidad", DomainConstants.NumericValidation.MinQuantity), nameof(quantity));

        BenefitId = benefitId;
        UserId = userId;
        Quantity = quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < DomainConstants.NumericValidation.MinQuantity)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cantidad", DomainConstants.NumericValidation.MinQuantity),
                nameof(quantity));

        Quantity = quantity;
        UpdateTimestamp();
    }
}

