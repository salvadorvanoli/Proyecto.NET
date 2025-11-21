using Mobile.Models;
using System.Net.Http.Json;

namespace Mobile.Services;

public class BenefitService : IBenefitService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BenefitService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<RedeemableBenefitDto>> GetRedeemableBenefitsAsync(int userId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("BenefitClient");
            
            System.Diagnostics.Debug.WriteLine($"[BenefitService] Fetching redeemable benefits for user {userId}");
            
            var response = await httpClient.GetAsync($"/api/benefits/redeemable/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var benefits = await response.Content.ReadFromJsonAsync<List<RedeemableBenefitDto>>();
                System.Diagnostics.Debug.WriteLine($"[BenefitService] Retrieved {benefits?.Count ?? 0} redeemable benefits");
                return benefits ?? new List<RedeemableBenefitDto>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[BenefitService] Error fetching benefits. Status: {response.StatusCode}, Error: {errorContent}");
                return new List<RedeemableBenefitDto>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BenefitService] Exception: {ex.Message}");
            return new List<RedeemableBenefitDto>();
        }
    }

    public async Task<RedeemBenefitResponseDto> RedeemBenefitAsync(int userId, int benefitId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("BenefitClient");
            
            var request = new
            {
                UserId = userId,
                BenefitId = benefitId
            };
            
            System.Diagnostics.Debug.WriteLine($"[BenefitService] Redeeming benefit {benefitId} for user {userId}");
            
            var response = await httpClient.PostAsJsonAsync("/api/benefits/redeem", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RedeemBenefitResponseDto>();
                System.Diagnostics.Debug.WriteLine($"[BenefitService] Successfully redeemed benefit");
                return result!;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[BenefitService] Error redeeming benefit. Status: {response.StatusCode}, Error: {errorContent}");
                throw new Exception($"Error al canjear el beneficio: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BenefitService] Exception: {ex.Message}");
            throw;
        }
    }
}
