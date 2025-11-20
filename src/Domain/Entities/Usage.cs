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

    /// <summary>
    /// Creates a usage with the quantity from the benefit.
    /// </summary>
    public Usage(int tenantId, Benefit benefit, int userId) : base(tenantId)
    {
        if (benefit == null)
            throw new ArgumentNullException(nameof(benefit));

        if (benefit.Id <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de beneficio"), nameof(benefit));

        if (userId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID de usuario"), nameof(userId));

        BenefitId = benefit.Id;
        UserId = userId;
        Quantity = benefit.Quotas;
        Benefit = benefit;
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

    /// <summary>
    /// Decrements the quantity by the specified amount.
    /// </summary>
    public void DecrementQuantity(int amount)
    {
        if (amount <= DomainConstants.NumericValidation.MinAmount)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cantidad", DomainConstants.NumericValidation.MinAmount),
                nameof(amount));

        if (amount > Quantity)
            throw new InvalidOperationException($"No se puede decrementar {amount} unidades. Cantidad disponible: {Quantity}");

        Quantity -= amount;
        UpdateTimestamp();
    }

    /// <summary>
    /// Checks if the usage has available quantity.
    /// </summary>
    public bool HasAvailableQuantity => Quantity > 0;
}

