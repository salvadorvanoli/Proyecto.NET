using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IUserApiService _userApiService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IUserApiService userApiService, ILogger<IndexModel> logger)
    {
        _userApiService = userApiService;
        _logger = logger;
    }

    public IEnumerable<UserDto> Users { get; set; } = Enumerable.Empty<UserDto>();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Users = await _userApiService.GetAllUsersAsync();
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
