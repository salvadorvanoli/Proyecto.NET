using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.Users;
using Web.BackOffice.Services;
using System.Security.Claims;

namespace Web.BackOffice.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IUserApiService _userApiService;
    private readonly ILogger<IndexModel> _logger;
    private const int PageSize = 10;

    public IndexModel(IUserApiService userApiService, ILogger<IndexModel> logger)
    {
        _userApiService = userApiService;
        _logger = logger;
    }

    public IEnumerable<UserResponse> Users { get; set; } = Enumerable.Empty<UserResponse>();
    public IEnumerable<UserResponse> DisplayedUsers { get; set; } = Enumerable.Empty<UserResponse>();

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalUsers { get; set; }

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
            Users = await _userApiService.GetUsersByTenantAsync();

            // Aplicar búsqueda si hay término de búsqueda
            var filteredUsers = Users.ToList();
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredUsers = filteredUsers.Where(u =>
                    u.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Calcular paginación
            TotalUsers = filteredUsers.Count;
            TotalPages = (int)Math.Ceiling(TotalUsers / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(pageNumber, TotalPages == 0 ? 1 : TotalPages));

            // Obtener usuarios de la página actual
            DisplayedUsers = filteredUsers
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            ErrorMessage = "Error al cargar los usuarios. Por favor, intente nuevamente.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            // Obtener el ID del usuario actual de los Claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                ErrorMessage = "No se pudo identificar al usuario actual.";
                return RedirectToPage();
            }

            // Validar que el usuario no intente eliminarse a sí mismo
            if (id == currentUserId)
            {
                ErrorMessage = "No puede eliminar su propia cuenta mientras tiene la sesión iniciada.";
                _logger.LogWarning("User {UserId} attempted to delete their own account", currentUserId);
                return RedirectToPage();
            }

            var deleted = await _userApiService.DeleteUserAsync(id);

            if (!deleted)
            {
                ErrorMessage = $"No se pudo eliminar el usuario con ID {id}. Puede que no exista.";
            }
            else
            {
                SuccessMessage = "Usuario eliminado correctamente.";
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            ErrorMessage = "Error al eliminar el usuario.";
            return RedirectToPage();
        }
    }
}
