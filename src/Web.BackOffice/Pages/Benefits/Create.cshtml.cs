using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.DTOs.Benefits;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Benefits;

public class CreateModel : PageModel
{
    private readonly IBenefitApiService _benefitApiService;
    private readonly IBenefitTypeApiService _benefitTypeApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IBenefitApiService benefitApiService, 
        IBenefitTypeApiService benefitTypeApiService,
        ILogger<CreateModel> logger)
    {
        _benefitApiService = benefitApiService;
        _benefitTypeApiService = benefitTypeApiService;
        _logger = logger;
    }

    [BindProperty]
    public BenefitRequest Benefit { get; set; } = new();

    public List<SelectListItem> BenefitTypes { get; set; } = new();
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadBenefitTypesAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create benefit page");
            ErrorMessage = "Ocurrió un error al cargar la página.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadBenefitTypesAsync();
            return Page();
        }

        // Validate dates if provided
        if (!string.IsNullOrWhiteSpace(Benefit.StartDate) || !string.IsNullOrWhiteSpace(Benefit.EndDate))
        {
            if (string.IsNullOrWhiteSpace(Benefit.StartDate) || string.IsNullOrWhiteSpace(Benefit.EndDate))
            {
                ModelState.AddModelError(string.Empty, "Debe proporcionar tanto la fecha de inicio como la de fin, o dejarlas ambas vacías para un beneficio permanente.");
                await LoadBenefitTypesAsync();
                return Page();
            }

            if (DateOnly.TryParse(Benefit.StartDate, out var startDate) && DateOnly.TryParse(Benefit.EndDate, out var endDate))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                
                if (startDate < today)
                {
                    ModelState.AddModelError(string.Empty, "La fecha de inicio no puede ser anterior a hoy.");
                    await LoadBenefitTypesAsync();
                    return Page();
                }
                
                if (endDate < today)
                {
                    ModelState.AddModelError(string.Empty, "La fecha de fin no puede ser anterior a hoy.");
                    await LoadBenefitTypesAsync();
                    return Page();
                }
                
                if (startDate > endDate)
                {
                    ModelState.AddModelError(string.Empty, "La fecha de inicio debe ser anterior o igual a la fecha de fin.");
                    await LoadBenefitTypesAsync();
                    return Page();
                }
            }
        }
        else
        {
            // Clear dates if permanent
            Benefit.StartDate = null;
            Benefit.EndDate = null;
        }

        try
        {
            await _benefitApiService.CreateBenefitAsync(Benefit);
            _logger.LogInformation("Created benefit for benefit type {BenefitTypeId}", Benefit.BenefitTypeId);
            TempData["SuccessMessage"] = "Beneficio creado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating benefit");
            ModelState.AddModelError(string.Empty, $"Error al crear el beneficio: {ex.Message}");
            await LoadBenefitTypesAsync();
            return Page();
        }
    }

    private async Task LoadBenefitTypesAsync()
    {
        var benefitTypes = await _benefitTypeApiService.GetBenefitTypesByTenantAsync();
        BenefitTypes = benefitTypes
            .Select(bt => new SelectListItem { Value = bt.Id.ToString(), Text = bt.Name })
            .ToList();
    }
}
