using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.BenefitTypes;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class CreateModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IBenefitTypeApiService benefitTypeApiService, ILogger<CreateModel> logger)
    {
        _benefitTypeApiService = benefitTypeApiService;
        _logger = logger;
    }

    [BindProperty]
    public BenefitTypeRequest BenefitType { get; set; } = new();
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _benefitTypeApiService.CreateBenefitTypeAsync(BenefitType);
            _logger.LogInformation("Benefit type '{Name}' created successfully", BenefitType.Name);
            TempData["SuccessMessage"] = "Tipo de beneficio creado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating benefit type '{Name}'", BenefitType.Name);
            ModelState.AddModelError(string.Empty, $"Error al crear el tipo de beneficio: {ex.Message}");
            return Page();
        }
    }
}
