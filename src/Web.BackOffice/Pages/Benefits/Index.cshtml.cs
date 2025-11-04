using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Benefits;

public class IndexModel : PageModel
{
    private readonly IBenefitApiService _benefitApiService;
    private readonly IBenefitTypeApiService _benefitTypeApiService;

    public IndexModel(IBenefitApiService benefitApiService, IBenefitTypeApiService benefitTypeApiService)
    {
        _benefitApiService = benefitApiService;
        _benefitTypeApiService = benefitTypeApiService;
    }

    public IEnumerable<BenefitDto> Benefits { get; set; } = new List<BenefitDto>();
    public List<SelectListItem> BenefitTypes { get; set; } = new();
    public int? SelectedBenefitTypeId { get; set; }
    public string? StatusFilter { get; set; }

    public async Task OnGetAsync(int? benefitTypeId, string? status)
    {
        SelectedBenefitTypeId = benefitTypeId;
        StatusFilter = status;

        // Load benefit types for filter dropdown
        var benefitTypes = await _benefitTypeApiService.GetBenefitTypesByTenantAsync();
        BenefitTypes = benefitTypes
            .Select(bt => new SelectListItem { Value = bt.Id.ToString(), Text = bt.Name })
            .ToList();

        // Get benefits based on filters
        IEnumerable<BenefitDto> benefits;

        if (benefitTypeId.HasValue)
        {
            benefits = await _benefitApiService.GetBenefitsByTypeAsync(benefitTypeId.Value);
        }
        else if (status == "active")
        {
            benefits = await _benefitApiService.GetActiveBenefitsAsync();
        }
        else
        {
            benefits = await _benefitApiService.GetBenefitsByTenantAsync();
        }

        Benefits = benefits.ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _benefitApiService.DeleteBenefitAsync(id);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar el beneficio.";
        }
        else
        {
            TempData["SuccessMessage"] = "Beneficio eliminado exitosamente.";
        }

        return RedirectToPage();
    }
}
