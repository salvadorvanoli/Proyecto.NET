using System.Net.Http.Json;
using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for benefit type API operations.
/// </summary>
public class BenefitTypeApiService : IBenefitTypeApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/benefittypes";

    public BenefitTypeApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BenefitTypeDto?> GetBenefitTypeByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BenefitTypeDto>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<BenefitTypeDto>> GetBenefitTypesByTenantAsync()
    {
        try
        {
            var benefitTypes = await _httpClient.GetFromJsonAsync<IEnumerable<BenefitTypeDto>>(BaseUrl);
            return benefitTypes ?? Enumerable.Empty<BenefitTypeDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<BenefitTypeDto>();
        }
    }

    public async Task<BenefitTypeDto?> CreateBenefitTypeAsync(CreateBenefitTypeDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BenefitTypeDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateBenefitTypeAsync(int id, UpdateBenefitTypeDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> DeleteBenefitTypeAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
