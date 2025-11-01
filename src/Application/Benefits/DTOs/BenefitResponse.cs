namespace Application.Benefits.DTOs;

/// <summary>
/// Response DTO for benefit information.
/// </summary>
public class BenefitResponse
{
    public int Id { get; set; }
    public BenefitTypeResponse BenefitType { get; set; } = null!;
    public DateRangeResponse? ValidityPeriod { get; set; }
    public int Quotas { get; set; }
    public bool IsValid { get; set; }
    public bool HasAvailableQuotas { get; set; }
    public bool CanBeConsumed { get; set; }
    public int TotalConsumed { get; set; }
    public List<ConsumptionResponse> Consumptions { get; set; } = new();
}

/// <summary>
/// Response DTO for benefit type information.
/// </summary>
public class BenefitTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for date range information.
/// </summary>
public class DateRangeResponse
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

/// <summary>
/// Response DTO for consumption information.
/// </summary>
public class ConsumptionResponse
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UsageResponse> Usages { get; set; } = new();
}

/// <summary>
/// Response DTO for usage information.
/// </summary>
public class UsageResponse
{
    public int Id { get; set; }
    public DateTime UsageDateTime { get; set; }
}
