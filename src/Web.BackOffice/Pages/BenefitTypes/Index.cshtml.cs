using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class IndexModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;
    private readonly ILogger<IndexModel> _logger;
    
    private const int PageSize = 10;

    public IndexModel(IBenefitTypeApiService benefitTypeApiService, ILogger<IndexModel> logger)
    {
        _benefitTypeApiService = benefitTypeApiService;
        _logger = logger;
    }

    public IEnumerable<BenefitTypeDto> BenefitTypes { get; set; } = new List<BenefitTypeDto>();
    public IEnumerable<BenefitTypeDto> DisplayedBenefitTypes { get; set; } = new List<BenefitTypeDto>();
    public string? SearchTerm { get; set; }
    
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalBenefitTypes { get; set; }
    
    [TempData]
    public string? SuccessMessage { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? searchTerm, int pageNumber = 1)
    {
        try
        {
            SearchTerm = searchTerm;
            CurrentPage = pageNumber < 1 ? 1 : pageNumber;

            var benefitTypes = await _benefitTypeApiService.GetBenefitTypesByTenantAsync();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                benefitTypes = benefitTypes.Where(bt =>
                    bt.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    bt.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            BenefitTypes = benefitTypes.OrderBy(bt => bt.Name).ToList();
            
            // Calculate pagination
            TotalBenefitTypes = BenefitTypes.Count();
            TotalPages = (int)Math.Ceiling(TotalBenefitTypes / (double)PageSize);
            
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }
            
            DisplayedBenefitTypes = BenefitTypes
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            _logger.LogInformation("Loaded {Count} benefit types (page {Page} of {TotalPages})", 
                DisplayedBenefitTypes.Count(), CurrentPage, TotalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading benefit types");
            ErrorMessage = "Ocurrió un error al cargar los tipos de beneficio.";
            BenefitTypes = new List<BenefitTypeDto>();
            DisplayedBenefitTypes = new List<BenefitTypeDto>();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var result = await _benefitTypeApiService.DeleteBenefitTypeAsync(id);
            
            if (!result)
            {
                ErrorMessage = "No se pudo eliminar el tipo de beneficio. Verifique que no tenga beneficios asociados.";
                _logger.LogWarning("Failed to delete benefit type with ID {Id}", id);
            }
            else
            {
                SuccessMessage = "Tipo de beneficio eliminado exitosamente.";
                _logger.LogInformation("Deleted benefit type with ID {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting benefit type with ID {Id}", id);
            ErrorMessage = "Ocurrió un error al eliminar el tipo de beneficio.";
        }

        return RedirectToPage();
    }
}
