using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.Roles;
using Web.BackOffice.Services;
using Domain.Constants;

namespace Web.BackOffice.Pages.Roles;

public class IndexModel : PageModel
{
    private readonly IRoleApiService _roleApiService;
    private readonly ILogger<IndexModel> _logger;
    private const int PageSize = 10;

    public IndexModel(IRoleApiService roleApiService, ILogger<IndexModel> logger)
    {
        _roleApiService = roleApiService;
        _logger = logger;
    }

    public IEnumerable<RoleResponse> Roles { get; set; } = Enumerable.Empty<RoleResponse>();
    public IEnumerable<RoleResponse> DisplayedRoles { get; set; } = Enumerable.Empty<RoleResponse>();

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalRoles { get; set; }

    // Búsqueda
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        try
        {
            Roles = await _roleApiService.GetRolesByTenantAsync();

            // Aplicar búsqueda si hay término de búsqueda
            var filteredRoles = Roles.ToList();
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredRoles = filteredRoles.Where(r =>
                    r.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Calcular paginación
            TotalRoles = filteredRoles.Count;
            TotalPages = (int)Math.Ceiling(TotalRoles / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(pageNumber, TotalPages == 0 ? 1 : TotalPages));

            // Obtener roles de la página actual
            DisplayedRoles = filteredRoles
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles");
            ErrorMessage = "Error al cargar los roles. Por favor, intente nuevamente.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            // Verificar que no se intente eliminar un rol protegido del sistema
            var role = await _roleApiService.GetRoleByIdAsync(id);
            if (role != null && DomainConstants.SystemRoles.IsProtectedRole(role.Name))
            {
                ErrorMessage = $"El rol '{role.Name}' es un rol del sistema y no puede ser eliminado.";
                return RedirectToPage();
            }

            var deleted = await _roleApiService.DeleteRoleAsync(id);

            if (!deleted)
            {
                ErrorMessage = $"No se pudo eliminar el rol con ID {id}. Puede que no exista.";
            }
            else
            {
                SuccessMessage = "Rol eliminado correctamente.";
            }

            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete role {RoleId}", id);
            ErrorMessage = ex.Message;
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            ErrorMessage = "Error al eliminar el rol. El rol puede tener usuarios asignados.";
            return RedirectToPage();
        }
    }
}
