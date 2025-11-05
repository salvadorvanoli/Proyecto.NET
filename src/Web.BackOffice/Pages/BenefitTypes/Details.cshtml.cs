using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class DetailsModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IBenefitTypeApiService benefitTypeApiService, ILogger<DetailsModel> logger)
    {
        _benefitTypeApiService = benefitTypeApiService;
        _logger = logger;
    }

    public BenefitTypeDto BenefitType { get; set; } = null!;
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var benefitType = await _benefitTypeApiService.GetBenefitTypeByIdAsync(id);
            
            if (benefitType == null)
            {
                _logger.LogWarning("Benefit type with ID {Id} not found", id);
                ErrorMessage = "Tipo de beneficio no encontrado.";
                return Page();
            }

            BenefitType = benefitType;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading benefit type with ID {Id}", id);
            ErrorMessage = "Ocurrió un error al cargar el tipo de beneficio.";
            return Page();
        }
    }
}
