namespace Shared.DTOs.Benefits;

/// <summary>
/// Response DTO after consuming a benefit.
/// </summary>
public class ConsumeBenefitResponse
{
    /// <summary>
    /// ID of the usage record created.
    /// </summary>
    public int UsageId { get; set; }

    /// <summary>
    /// ID of the consumption record created.
    /// </summary>
    public int ConsumptionId { get; set; }

    /// <summary>
    /// ID of the benefit consumed.
    /// </summary>
    public int BenefitId { get; set; }

    /// <summary>
    /// Name of the benefit type consumed.
    /// </summary>
    public string BenefitTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity consumed.
    /// </summary>
    public int QuantityConsumed { get; set; }

    /// <summary>
    /// Remaining quotas for the benefit.
    /// </summary>
    public int RemainingQuotas { get; set; }

    /// <summary>
    /// Date and time when the consumption occurred.
    /// </summary>
    public DateTime ConsumptionDateTime { get; set; }

    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
