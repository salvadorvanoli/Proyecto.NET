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

    public EditModel(IBenefitApiService benefitApiService, IBenefitTypeApiService benefitTypeApiService)
    {
        _benefitApiService = benefitApiService;
        _benefitTypeApiService = benefitTypeApiService;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public UpdateBenefitDto Benefit { get; set; } = new();

    public List<SelectListItem> BenefitTypes { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var benefit = await _benefitApiService.GetBenefitByIdAsync(id);
        
        if (benefit == null)
        {
            TempData["ErrorMessage"] = "Beneficio no encontrado.";
            return RedirectToPage("./Index");
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
                TempData["ErrorMessage"] = "No se pudo actualizar el beneficio.";
                await LoadBenefitTypesAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Beneficio actualizado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
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
