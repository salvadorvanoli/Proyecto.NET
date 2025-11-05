using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.AccessRules;

public class IndexModel : PageModel
{
    private readonly IAccessRuleApiService _accessRuleApiService;
    private readonly IRoleApiService _roleApiService;
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ILogger<IndexModel> _logger;
    
    private const int PageSize = 10;

    public IndexModel(
        IAccessRuleApiService accessRuleApiService,
        IRoleApiService roleApiService,
        IControlPointApiService controlPointApiService,
        ILogger<IndexModel> logger)
    {
        _accessRuleApiService = accessRuleApiService;
        _roleApiService = roleApiService;
        _controlPointApiService = controlPointApiService;
        _logger = logger;
    }

    public IEnumerable<AccessRuleDto> AccessRules { get; set; } = new List<AccessRuleDto>();
    public IEnumerable<AccessRuleDto> DisplayedAccessRules { get; set; } = new List<AccessRuleDto>();
    public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();
    public IEnumerable<ControlPointDto> ControlPoints { get; set; } = new List<ControlPointDto>();
    
    public int? SelectedRoleId { get; set; }
    public int? SelectedControlPointId { get; set; }
    public string? ActiveFilter { get; set; }
    
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalAccessRules { get; set; }
    
    [TempData]
    public string? SuccessMessage { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int? roleId, int? controlPointId, string? active, int pageNumber = 1)
    {
        try
        {
            SelectedRoleId = roleId;
            SelectedControlPointId = controlPointId;
            ActiveFilter = active;
            CurrentPage = pageNumber < 1 ? 1 : pageNumber;

            // Load all data in parallel
            var accessRulesTask = _accessRuleApiService.GetAccessRulesByTenantAsync();
            var rolesTask = _roleApiService.GetAllRolesAsync();
            var controlPointsTask = _controlPointApiService.GetControlPointsByTenantAsync();

            await Task.WhenAll(accessRulesTask, rolesTask, controlPointsTask);

            var accessRules = await accessRulesTask;
            Roles = await rolesTask;
            ControlPoints = await controlPointsTask;

            // Apply filters
            if (SelectedRoleId.HasValue)
            {
                accessRules = accessRules.Where(ar => ar.RoleIds.Contains(SelectedRoleId.Value));
            }

            if (SelectedControlPointId.HasValue)
            {
                accessRules = accessRules.Where(ar => ar.ControlPointIds.Contains(SelectedControlPointId.Value));
            }

            if (!string.IsNullOrWhiteSpace(ActiveFilter))
            {
                if (ActiveFilter.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    accessRules = accessRules.Where(ar => ar.IsActive);
                }
                else if (ActiveFilter.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    accessRules = accessRules.Where(ar => !ar.IsActive);
                }
            }

            AccessRules = accessRules.OrderByDescending(ar => ar.IsActive).ThenByDescending(ar => ar.CreatedAt).ToList();
            
            // Calculate pagination
            TotalAccessRules = AccessRules.Count();
            TotalPages = (int)Math.Ceiling(TotalAccessRules / (double)PageSize);
            
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }
            
            DisplayedAccessRules = AccessRules
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            _logger.LogInformation("Loaded {Count} access rules (page {Page} of {TotalPages})", 
                DisplayedAccessRules.Count(), CurrentPage, TotalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading access rules");
            ErrorMessage = "Ocurrió un error al cargar las reglas de acceso.";
            AccessRules = new List<AccessRuleDto>();
            DisplayedAccessRules = new List<AccessRuleDto>();
            Roles = new List<RoleDto>();
            ControlPoints = new List<ControlPointDto>();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var result = await _accessRuleApiService.DeleteAccessRuleAsync(id);
            
            if (!result)
            {
                ErrorMessage = "No se pudo eliminar la regla de acceso.";
                _logger.LogWarning("Failed to delete access rule with ID {Id}", id);
            }
            else
            {
                SuccessMessage = "Regla de acceso eliminada exitosamente.";
                _logger.LogInformation("Deleted access rule with ID {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting access rule with ID {Id}", id);
            ErrorMessage = "Ocurrió un error al eliminar la regla de acceso.";
        }

        return RedirectToPage();
    }
}
