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
    private readonly ILogger<IndexModel> _logger;
    
    private const int PageSize = 10;

    public IndexModel(
        IBenefitApiService benefitApiService, 
        IBenefitTypeApiService benefitTypeApiService,
        ILogger<IndexModel> logger)
    {
        _benefitApiService = benefitApiService;
        _benefitTypeApiService = benefitTypeApiService;
        _logger = logger;
    }

    public IEnumerable<BenefitDto> Benefits { get; set; } = new List<BenefitDto>();
    public IEnumerable<BenefitDto> DisplayedBenefits { get; set; } = new List<BenefitDto>();
    public List<SelectListItem> BenefitTypes { get; set; } = new();
    public int? SelectedBenefitTypeId { get; set; }
    public string? StatusFilter { get; set; }
    
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalBenefits { get; set; }
    
    [TempData]
    public string? SuccessMessage { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int? benefitTypeId, string? status, int pageNumber = 1)
    {
        try
        {
            SelectedBenefitTypeId = benefitTypeId;
            StatusFilter = status;
            CurrentPage = pageNumber < 1 ? 1 : pageNumber;

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
            
            // Calculate pagination
            TotalBenefits = Benefits.Count();
            TotalPages = (int)Math.Ceiling(TotalBenefits / (double)PageSize);
            
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }
            
            DisplayedBenefits = Benefits
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            _logger.LogInformation("Loaded {Count} benefits (page {Page} of {TotalPages})", 
                DisplayedBenefits.Count(), CurrentPage, TotalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading benefits");
            ErrorMessage = "Ocurrió un error al cargar los beneficios.";
            Benefits = new List<BenefitDto>();
            DisplayedBenefits = new List<BenefitDto>();
            BenefitTypes = new List<SelectListItem>();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var result = await _benefitApiService.DeleteBenefitAsync(id);
            
            if (!result)
            {
                ErrorMessage = "No se pudo eliminar el beneficio.";
                _logger.LogWarning("Failed to delete benefit with ID {Id}", id);
            }
            else
            {
                SuccessMessage = "Beneficio eliminado exitosamente.";
                _logger.LogInformation("Deleted benefit with ID {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting benefit with ID {Id}", id);
            ErrorMessage = "Ocurrió un error al eliminar el beneficio.";
        }

        return RedirectToPage();
    }
}
