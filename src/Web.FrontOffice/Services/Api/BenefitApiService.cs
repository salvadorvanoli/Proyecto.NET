using Application.Benefits.DTOs;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of the benefit API service.
/// </summary>
public class BenefitApiService : IBenefitApiService
{
    private readonly HttpClient _httpClient;

    public BenefitApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId)
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync($"api/benefits/user/{userId}");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<BenefitResponse>>();
        return benefits ?? new List<BenefitResponse>();
    }

    public async Task<List<BenefitResponse>> GetAllBenefitsAsync()
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync("api/benefits");
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<List<BenefitResponse>>();
        return benefits ?? new List<BenefitResponse>();
    }

    public async Task<BenefitResponse?> GetBenefitByIdAsync(int benefitId)
    {
        // TODO: El header X-Tenant-Id debería venir de la autenticación del usuario
        var response = await _httpClient.GetAsync($"api/benefits/{benefitId}");
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BenefitResponse>();
    }
}
