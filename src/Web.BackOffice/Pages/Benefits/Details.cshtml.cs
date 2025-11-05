using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Benefits;

public class DetailsModel : PageModel
{
    private readonly IBenefitApiService _benefitApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IBenefitApiService benefitApiService, ILogger<DetailsModel> logger)
    {
        _benefitApiService = benefitApiService;
        _logger = logger;
    }

    public BenefitDto Benefit { get; set; } = null!;
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var benefit = await _benefitApiService.GetBenefitByIdAsync(id);
            
            if (benefit == null)
            {
                ErrorMessage = "Beneficio no encontrado.";
                _logger.LogWarning("Benefit with ID {Id} not found", id);
                return Page();
            }

            Benefit = benefit;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading benefit with ID {Id}", id);
            ErrorMessage = "Ocurrió un error al cargar el beneficio.";
            return Page();
        }
    }
}
