using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.Users;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Users;

public class DetailsModel : PageModel
{
    private readonly IUserApiService _userApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IUserApiService userApiService, ILogger<DetailsModel> logger)
    {
        _userApiService = userApiService;
        _logger = logger;
    }

    public new UserResponse? User { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            User = await _userApiService.GetUserByIdAsync(id);

            if (User == null)
            {
                ErrorMessage = $"Usuario con ID {id} no encontrado.";
                return Page();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user details for ID {UserId}", id);
            ErrorMessage = "Error al cargar los detalles del usuario.";
            return Page();
        }
    }
}
