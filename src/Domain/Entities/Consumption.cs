using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a consumption of a benefit by a user.
/// </summary>
public class Consumption : BaseEntity
{
    /// <summary>
    /// Amount consumed.
    /// </summary>
    public int Amount { get; protected set; }

    /// <summary>
    /// Date and time when the consumption occurred.
    /// </summary>
    public DateTime ConsumptionDateTime { get; protected set; }

    /// <summary>
    /// Foreign key to the usage this consumption belongs to.
    /// </summary>
    public int UsageId { get; protected set; }

    protected Consumption() : base()
    {
    }

    public Consumption(int tenantId, int amount, DateTime consumptionDateTime, int usageId) : base(tenantId)
    {
        if (amount <= DomainConstants.NumericValidation.MinAmount)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Amount", DomainConstants.NumericValidation.MinAmount),
                nameof(amount));

        if (usageId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Usage ID"),
                nameof(usageId));

        Amount = amount;
        ConsumptionDateTime = consumptionDateTime;
        UsageId = usageId;
    }

    /// <summary>
    /// Updates the consumed amount.
    /// </summary>
    public void UpdateAmount(int amount)
    {
        if (amount <= DomainConstants.NumericValidation.MinAmount)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanOrEqualTo, "Amount", DomainConstants.NumericValidation.MinAmount),
                nameof(amount));

        Amount = amount;
        UpdateTimestamp();
    }
}

