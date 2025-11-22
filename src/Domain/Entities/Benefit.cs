using Domain.Constants;
using Domain.DataTypes;

namespace Domain.Entities;

/// <summary>
/// Represents a benefit that can be consumed by users.
/// </summary>
public class Benefit : BaseEntity
{
    /// <summary>
    /// Validity period for this benefit (can be null for permanent benefits).
    /// </summary>
    public DateRange? ValidityPeriod { get; protected set; }

    /// <summary>
    /// Number of available quotas for this benefit.
    /// </summary>
    public int Quotas { get; protected set; }

    /// <summary>
    /// Quantity associated with this benefit.
    /// </summary>
    public int Quantity { get; protected set; }

    /// <summary>
    /// Foreign key to the benefit type.
    /// </summary>
    public int BenefitTypeId { get; protected set; }

    /// <summary>
    /// Indicates if the benefit is active (for soft delete).
    /// </summary>
    public bool Active { get; protected set; }

    // Navigation properties
    public virtual BenefitType BenefitType { get; protected set; } = null!;

    protected Benefit() : base()
    {
    }

    public Benefit(int tenantId, int benefitTypeId, int quotas, int quantity, DateRange? validityPeriod = null) : base(tenantId)
    {
        if (benefitTypeId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "ID"),
                nameof(benefitTypeId));

        if (quotas < DomainConstants.NumericValidation.MinQuota)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cuotas", DomainConstants.NumericValidation.MinQuota),
                nameof(quotas));
        
        if (quantity < DomainConstants.NumericValidation.MinQuantity)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cantidad", DomainConstants.NumericValidation.MinQuantity),
                nameof(quantity));

        BenefitTypeId = benefitTypeId;
        Quotas = quotas;
        Quantity = quantity;
        ValidityPeriod = validityPeriod;
        Active = true;
    }

    /// <summary>
    /// Updates the validity period of the benefit.
    /// </summary>
    public void UpdateValidityPeriod(DateRange? validityPeriod)
    {
        ValidityPeriod = validityPeriod;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the number of quotas available.
    /// </summary>
    public void UpdateQuotas(int quotas)
    {
        if (quotas <= DomainConstants.NumericValidation.MinQuota)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cuotas", DomainConstants.NumericValidation.MinQuota),
                nameof(quotas));

        Quotas += quotas;
        UpdateTimestamp();
    }

    /// <summary>
    /// Increases the number of quotas.
    /// </summary>
    public void AddQuotas(int amount)
    {
        if (amount <= DomainConstants.NumericValidation.MinAmount)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cantidad", DomainConstants.NumericValidation.MinAmount),
                nameof(amount));

        Quotas += amount;
        UpdateTimestamp();
    }

    /// <summary>
    /// Decreases the number of quotas.
    /// </summary>
    public void ConsumeQuotas(int amount)
    {
        if (amount <= DomainConstants.NumericValidation.MinAmount)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Cantidad", DomainConstants.NumericValidation.MinAmount),
                nameof(amount));

        if (amount > Quotas)
            throw new InvalidOperationException("No hay suficientes cuotas disponibles.");

        Quotas -= amount;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the quantity associated with the benefit.
    /// </summary>
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
    /// Checks if the benefit is currently valid based on the validity period.
    /// </summary>
    public bool IsValid => ValidityPeriod?.IsActive != false;

    /// <summary>
    /// Checks if there are available quotas.
    /// </summary>
    public bool HasAvailableQuotas => Quotas > 0;

    /// <summary>
    /// Checks if the benefit can be consumed (is valid and has quotas).
    /// </summary>
    public bool CanBeConsumed => IsValid && HasAvailableQuotas && Active;

    /// <summary>
    /// Deactivates the benefit (soft delete).
    /// </summary>
    public void Deactivate()
    {
        Active = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Activates the benefit.
    /// </summary>
    public void Activate()
    {
        Active = true;
        UpdateTimestamp();
    }
}

