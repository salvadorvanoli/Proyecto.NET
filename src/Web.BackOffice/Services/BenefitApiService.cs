using System.Net.Http.Json;
using Shared.DTOs.Benefits;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for interacting with the Benefits API.
/// </summary>
public class BenefitApiService : IBenefitApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/benefits";

    public BenefitApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BenefitResponse?> GetBenefitByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BenefitResponse>();
    }

    public async Task<IEnumerable<BenefitResponse>> GetBenefitsByTenantAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/by-tenant");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<BenefitResponse>>() 
               ?? Enumerable.Empty<BenefitResponse>();
    }

    public async Task<IEnumerable<BenefitResponse>> GetBenefitsByTypeAsync(int benefitTypeId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/by-type/{benefitTypeId}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<BenefitResponse>>() 
               ?? Enumerable.Empty<BenefitResponse>();
    }

    public async Task<IEnumerable<BenefitResponse>> GetActiveBenefitsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/active");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<BenefitResponse>>() 
               ?? Enumerable.Empty<BenefitResponse>();
    }

    public async Task<BenefitResponse> CreateBenefitAsync(BenefitRequest benefit)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, benefit);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<BenefitResponse>() 
               ?? throw new InvalidOperationException("Failed to create benefit");
    }

    public async Task<bool> UpdateBenefitAsync(int id, BenefitRequest benefit)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", benefit);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBenefitAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
