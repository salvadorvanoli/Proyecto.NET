using System.Net.Http.Json;
using Web.BackOffice.Models;

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

    public async Task<BenefitDto?> GetBenefitByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BenefitDto>();
    }

    public async Task<IEnumerable<BenefitDto>> GetBenefitsByTenantAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/by-tenant");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<BenefitDto>>() 
               ?? Enumerable.Empty<BenefitDto>();
    }

    public async Task<IEnumerable<BenefitDto>> GetBenefitsByTypeAsync(int benefitTypeId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/by-type/{benefitTypeId}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<BenefitDto>>() 
               ?? Enumerable.Empty<BenefitDto>();
    }

    public async Task<IEnumerable<BenefitDto>> GetActiveBenefitsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/active");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<BenefitDto>>() 
               ?? Enumerable.Empty<BenefitDto>();
    }

    public async Task<BenefitDto> CreateBenefitAsync(CreateBenefitDto benefit)
    {
        var createRequest = new
        {
            benefitTypeId = benefit.BenefitTypeId,
            quotas = benefit.Quotas,
            startDate = benefit.IsPermanent ? null : benefit.StartDate,
            endDate = benefit.IsPermanent ? null : benefit.EndDate
        };

        var response = await _httpClient.PostAsJsonAsync(BaseUrl, createRequest);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<BenefitDto>() 
               ?? throw new InvalidOperationException("Failed to create benefit");
    }

    public async Task<bool> UpdateBenefitAsync(int id, UpdateBenefitDto benefit)
    {
        var updateRequest = new
        {
            benefitTypeId = benefit.BenefitTypeId,
            quotas = benefit.Quotas,
            startDate = benefit.IsPermanent ? null : benefit.StartDate,
            endDate = benefit.IsPermanent ? null : benefit.EndDate
        };

        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateRequest);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBenefitAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
