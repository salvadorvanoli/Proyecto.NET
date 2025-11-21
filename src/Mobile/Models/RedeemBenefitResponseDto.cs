namespace Mobile.Models;

public class RedeemBenefitResponseDto
{
    public int UsageId { get; set; }
    public int ConsumptionId { get; set; }
    public int BenefitId { get; set; }
    public int UserId { get; set; }
    public int RemainingUsageQuantity { get; set; }
    public int RemainingBenefitQuotas { get; set; }
    public DateTime RedeemedAt { get; set; }
    public bool IsNewUsage { get; set; }
    public string Message { get; set; } = string.Empty;
    
    // Propiedad computed para indicar éxito (siempre true si se recibe respuesta)
    public bool Success => true;
}
