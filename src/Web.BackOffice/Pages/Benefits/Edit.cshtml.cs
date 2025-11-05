using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Benefits;

public class EditModel : PageModel
{
    private readonly IBenefitApiService _benefitApiService;
    private readonly IBenefitTypeApiService _benefitTypeApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        IBenefitApiService benefitApiService, 
        IBenefitTypeApiService benefitTypeApiService,
        ILogger<EditModel> logger)
    {
        _benefitApiService = benefitApiService;
        _benefitTypeApiService = benefitTypeApiService;
        _logger = logger;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public UpdateBenefitDto Benefit { get; set; } = new();

    public List<SelectListItem> BenefitTypes { get; set; } = new();
    
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

            Id = benefit.Id;
            Benefit = new UpdateBenefitDto
            {
                BenefitTypeId = benefit.BenefitTypeId,
                Quotas = benefit.Quotas,
                StartDate = benefit.StartDate,
                EndDate = benefit.EndDate,
                IsPermanent = benefit.IsPermanent
            };

            await LoadBenefitTypesAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading benefit with ID {Id} for edit", id);
            ErrorMessage = "Ocurrió un error al cargar el beneficio.";
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

        // Validate dates if not permanent
        if (!Benefit.IsPermanent)
        {
            if (string.IsNullOrWhiteSpace(Benefit.StartDate) || string.IsNullOrWhiteSpace(Benefit.EndDate))
            {
                ModelState.AddModelError(string.Empty, "Las fechas de inicio y fin son requeridas para beneficios no permanentes.");
                await LoadBenefitTypesAsync();
                return Page();
            }

            if (DateOnly.TryParse(Benefit.StartDate, out var startDate) && DateOnly.TryParse(Benefit.EndDate, out var endDate))
            {
                if (startDate > endDate)
                {
                    ModelState.AddModelError(string.Empty, "La fecha de inicio debe ser anterior o igual a la fecha de fin.");
                    await LoadBenefitTypesAsync();
                    return Page();
                }
            }
        }

        try
        {
            var result = await _benefitApiService.UpdateBenefitAsync(Id, Benefit);
            
            if (!result)
            {
                ErrorMessage = "No se pudo actualizar el beneficio.";
                _logger.LogWarning("Failed to update benefit with ID {Id}", Id);
                await LoadBenefitTypesAsync();
                return Page();
            }

            _logger.LogInformation("Updated benefit with ID {Id}", Id);
            TempData["SuccessMessage"] = "Beneficio actualizado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating benefit with ID {Id}", Id);
            ModelState.AddModelError(string.Empty, $"Error al actualizar el beneficio: {ex.Message}");
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
