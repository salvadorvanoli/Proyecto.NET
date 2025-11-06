using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.DTOs.AccessRules;
using Shared.DTOs.Roles;
using Shared.DTOs.ControlPoints;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.AccessRules;

public class CreateModel : PageModel
{
    private readonly IAccessRuleApiService _accessRuleApiService;
    private readonly IRoleApiService _roleApiService;
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IAccessRuleApiService accessRuleApiService,
        IRoleApiService roleApiService,
        IControlPointApiService controlPointApiService,
        ILogger<CreateModel> logger)
    {
        _accessRuleApiService = accessRuleApiService;
        _roleApiService = roleApiService;
        _controlPointApiService = controlPointApiService;
        _logger = logger;
    }

    [BindProperty]
    public AccessRuleRequest AccessRule { get; set; } = new();

    public MultiSelectList Roles { get; set; } = new(new List<RoleResponse>(), "Id", "Name");
    public MultiSelectList ControlPoints { get; set; } = new(new List<ControlPointResponse>(), "Id", "Name");

    [BindProperty]
    public bool Use24x7 { get; set; } = true;

    [BindProperty]
    public bool UsePermanent { get; set; } = true;
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadSelectListsAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data for access rule creation");
            ErrorMessage = "Ocurrió un error al cargar los datos necesarios.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Clear time values if 24x7 is checked
            if (Use24x7)
            {
                AccessRule.StartTime = null;
                AccessRule.EndTime = null;
            }

            // Clear date values if permanent is checked
            if (UsePermanent)
            {
                AccessRule.StartDate = null;
                AccessRule.EndDate = null;
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            // Validate at least one role and control point selected
            if (AccessRule.RoleIds == null || !AccessRule.RoleIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un rol.");
                await LoadSelectListsAsync();
                return Page();
            }

            if (AccessRule.ControlPointIds == null || !AccessRule.ControlPointIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un punto de control.");
                await LoadSelectListsAsync();
                return Page();
            }

            // Validate time range if not 24x7
            if (!Use24x7 && (string.IsNullOrWhiteSpace(AccessRule.StartTime) || string.IsNullOrWhiteSpace(AccessRule.EndTime)))
            {
                ModelState.AddModelError(string.Empty, "Debe especificar un rango horario o marcar acceso 24/7.");
                await LoadSelectListsAsync();
                return Page();
            }

            // Validate date range if not permanent
            if (!UsePermanent)
            {
                if (!AccessRule.StartDate.HasValue || !AccessRule.EndDate.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Debe especificar un periodo de validez o marcar como permanente.");
                    await LoadSelectListsAsync();
                    return Page();
                }

                var today = DateTime.Today;
                
                if (AccessRule.StartDate.Value.Date < today)
                {
                    ModelState.AddModelError(string.Empty, "La fecha de inicio no puede ser anterior a hoy.");
                    await LoadSelectListsAsync();
                    return Page();
                }
                
                if (AccessRule.EndDate.Value.Date < today)
                {
                    ModelState.AddModelError(string.Empty, "La fecha de fin no puede ser anterior a hoy.");
                    await LoadSelectListsAsync();
                    return Page();
                }
                
                if (AccessRule.StartDate.Value > AccessRule.EndDate.Value)
                {
                    ModelState.AddModelError(string.Empty, "La fecha de inicio debe ser anterior o igual a la fecha de fin.");
                    await LoadSelectListsAsync();
                    return Page();
                }
            }

            var result = await _accessRuleApiService.CreateAccessRuleAsync(AccessRule);

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo crear la regla de acceso.");
                await LoadSelectListsAsync();
                return Page();
            }

            _logger.LogInformation("Access rule created successfully with ID {Id}", result.Id);
            TempData["SuccessMessage"] = "Regla de acceso creada exitosamente.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating access rule");
            ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la regla de acceso.");
            await LoadSelectListsAsync();
            return Page();
        }
    }

    private async Task LoadSelectListsAsync()
    {
        try
        {
            var rolesTask = _roleApiService.GetAllRolesAsync();
            var controlPointsTask = _controlPointApiService.GetControlPointsByTenantAsync();

            await Task.WhenAll(rolesTask, controlPointsTask);

            var roles = await rolesTask;
            var controlPoints = await controlPointsTask;

            Roles = new MultiSelectList(roles, "Id", "Name", AccessRule.RoleIds);
            ControlPoints = new MultiSelectList(
                controlPoints.Select(cp => new { cp.Id, DisplayName = $"{cp.Name} ({cp.SpaceName})" }),
                "Id",
                "DisplayName",
                AccessRule.ControlPointIds
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles and control points");
            Roles = new MultiSelectList(new List<RoleResponse>(), "Id", "Name");
            ControlPoints = new MultiSelectList(new List<ControlPointResponse>(), "Id", "Name");
            throw;
        }
    }
}
