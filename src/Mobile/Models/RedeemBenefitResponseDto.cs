namespace Mobile.Models;

public class RedeemBenefitResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RemainingQuantity { get; set; }
}
