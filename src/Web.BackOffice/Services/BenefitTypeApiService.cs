using System.Net.Http.Json;
using Shared.DTOs.BenefitTypes;

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

    public async Task<BenefitTypeResponse?> GetBenefitTypeByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BenefitTypeResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<BenefitTypeResponse>> GetBenefitTypesByTenantAsync()
    {
        try
        {
            var benefitTypes = await _httpClient.GetFromJsonAsync<IEnumerable<BenefitTypeResponse>>(BaseUrl);
            return benefitTypes ?? Enumerable.Empty<BenefitTypeResponse>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<BenefitTypeResponse>();
        }
    }

    public async Task<BenefitTypeResponse?> CreateBenefitTypeAsync(BenefitTypeRequest dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BenefitTypeResponse>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateBenefitTypeAsync(int id, BenefitTypeRequest dto)
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
