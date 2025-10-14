using Domain.Constants;

namespace Domain.Entities;

/// <summary>
/// Represents a usage event of a consumption.
/// </summary>
public class Usage : BaseEntity
{
    /// <summary>
    /// Date and time when the usage occurred.
    /// </summary>
    public DateTime UsageDateTime { get; protected set; }

    /// <summary>
    /// Foreign key to the consumption this usage belongs to.
    /// </summary>
    public int ConsumptionId { get; protected set; }

    // Navigation properties
    public virtual Consumption Consumption { get; protected set; } = null!;

    protected Usage() : base()
    {
    }

    public Usage(int tenantId, int consumptionId, DateTime usageDateTime) : base(tenantId)
    {
        if (consumptionId <= DomainConstants.NumericValidation.TransientEntityId)
            throw new ArgumentException(string.Format(DomainConstants.ErrorMessages.MustBeGreaterThanZero, "Consumption ID"), nameof(consumptionId));

        ConsumptionId = consumptionId;
        UsageDateTime = usageDateTime;
    }

    /// <summary>
    /// Creates a usage with the current timestamp.
    /// </summary>
    public static Usage CreateNow(int tenantId, int consumptionId)
    {
        return new Usage(tenantId, consumptionId, DateTime.UtcNow);
    }
}

