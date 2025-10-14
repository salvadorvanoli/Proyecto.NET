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
    /// Foreign key to the benefit type.
    /// </summary>
    public int BenefitTypeId { get; protected set; }

    // Navigation properties
    public virtual BenefitType BenefitType { get; protected set; } = null!;
    public virtual ICollection<Consumption> Consumptions { get; protected set; } = new List<Consumption>();

    protected Benefit() : base()
    {
    }

    public Benefit(int tenantId, int benefitTypeId, int quotas, DateRange? validityPeriod = null) : base(tenantId)
    {
        if (benefitTypeId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Benefit type ID"),
                nameof(benefitTypeId));

        if (quotas <= DomainConstants.NumericValidation.MinQuota)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Quotas"),
                nameof(quotas));

        BenefitTypeId = benefitTypeId;
        Quotas = quotas;
        ValidityPeriod = validityPeriod;
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
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Quotas"),
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
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Amount"),
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
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Amount"),
                nameof(amount));

        if (amount > Quotas)
            throw new InvalidOperationException("Not enough quotas available.");

        Quotas -= amount;
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
    public bool CanBeConsumed => IsValid && HasAvailableQuotas;

    /// <summary>
    /// Gets the total consumed amount for this benefit.
    /// </summary>
    public int TotalConsumed => Consumptions.Sum(c => c.Amount);
}

