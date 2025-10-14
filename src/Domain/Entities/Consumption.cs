using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a consumption of a benefit by a user.
/// </summary>
public class Consumption : BaseEntity
{
    /// <summary>
    /// Amount consumed from the benefit.
    /// </summary>
    public int Amount { get; protected set; }

    /// <summary>
    /// Foreign key to the benefit being consumed.
    /// </summary>
    public int BenefitId { get; protected set; }

    /// <summary>
    /// Foreign key to the user who made the consumption.
    /// </summary>
    public int UserId { get; protected set; }

    // Navigation properties
    public virtual Benefit Benefit { get; protected set; } = null!;
    public virtual User User { get; protected set; } = null!;
    public virtual ICollection<Usage> Usages { get; protected set; } = new List<Usage>();

    protected Consumption() : base()
    {
    }

    public Consumption(int tenantId, int amount, int benefitId, int userId) : base(tenantId)
    {
        if (amount <= DomainConstants.NumericValidation.MinAmount)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Amount"),
                nameof(amount));

        if (benefitId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Benefit ID"),
                nameof(benefitId));

        if (userId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "User ID"),
                nameof(userId));

        Amount = amount;
        BenefitId = benefitId;
        UserId = userId;
    }

    /// <summary>
    /// Updates the consumed amount.
    /// </summary>
    public void UpdateAmount(int amount)
    {
        if (amount <= DomainConstants.NumericValidation.MinAmount)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Amount"),
                nameof(amount));

        Amount = amount;
        UpdateTimestamp();
    }

    /// <summary>
    /// Adds a usage record to this consumption.
    /// </summary>
    public void AddUsage(Usage usage)
    {
        if (usage == null)
            throw new ArgumentNullException(nameof(usage));

        if (usage.TenantId != TenantId)
            throw new ArgumentException(
                string.Format(DomainConstants.ErrorMessages.MustBelongToSameTenant, "Usage"),
                nameof(usage));

        if (!Usages.Contains(usage))
        {
            Usages.Add(usage);
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Gets the total usage count for this consumption.
    /// </summary>
    public int TotalUsages => Usages.Count;
}

