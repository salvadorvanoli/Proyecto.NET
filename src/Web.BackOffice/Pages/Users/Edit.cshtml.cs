using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Users;

public class EditModel : PageModel
{
    private readonly IUserApiService _userApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IUserApiService userApiService, ILogger<EditModel> logger)
    {
        _userApiService = userApiService;
        _logger = logger;
    }

    [BindProperty]
    public new EditUserInputModel User { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public class EditUserInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var userDto = await _userApiService.GetUserByIdAsync(id);

            if (userDto == null)
            {
                ErrorMessage = $"Usuario con ID {id} no encontrado.";
                return Page();
            }

            User = new EditUserInputModel
            {
                Id = userDto.Id,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                DateOfBirth = userDto.DateOfBirth
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for editing, ID {UserId}", id);
            ErrorMessage = "Error al cargar el usuario para editar.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var updateDto = new UpdateUserDto
            {
                Email = User.Email,
                FirstName = User.FirstName,
                LastName = User.LastName,
                DateOfBirth = User.DateOfBirth
            };

            await _userApiService.UpdateUserAsync(User.Id, updateDto);

            return RedirectToPage("/Users/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", User.Id);
            ErrorMessage = "Error al actualizar el usuario.";
            return Page();
        }
    }
}
