using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Benefits;

public class CreateModel : PageModel
{
    private readonly IBenefitApiService _benefitApiService;
    private readonly IBenefitTypeApiService _benefitTypeApiService;

    public CreateModel(IBenefitApiService benefitApiService, IBenefitTypeApiService benefitTypeApiService)
    {
        _benefitApiService = benefitApiService;
        _benefitTypeApiService = benefitTypeApiService;
    }

    [BindProperty]
    public CreateBenefitDto Benefit { get; set; } = new();

    public List<SelectListItem> BenefitTypes { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadBenefitTypesAsync();
        Benefit.IsPermanent = true; // Default to permanent
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
            await _benefitApiService.CreateBenefitAsync(Benefit);
            TempData["SuccessMessage"] = "Beneficio creado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
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
